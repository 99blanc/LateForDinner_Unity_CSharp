using R3;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Token.ID;
using Token.DATA;
using Token.PRIORITY;

public class PlayerControl : AgentControl<IPlayerView, PlayerData, PlayerID>, IUseLadder
{
    public new ModulePrority priority => ModulePrority.PLAYER_CONTROL;
    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    public PlayerJumpState jumpState { get; private set; }
    public PlayerFallState fallState { get; private set; }
    public PlayerDashState dashState { get; private set; }
    public PlayerLadderState ladderState { get; private set; }
    private Vector2 lastMoveDirection;
    private float lastMoveInputTime;
    private bool isCoolingDown;

    protected override void Behaviors()
    {
        SetBehavior(new MoveBehavior<IMoveData>());
        SetBehavior(new JumpBehavior<IJumpData>());
        SetBehavior(new FallBehavior<IJumpData>());
        SetBehavior(new DashBehavior<IDashData>());
        SetBehavior(new GravityBehavior<IPhysicsData>());
        SetBehavior(new LadderBehavior<ILadderData>());
    }

    public override void Setup(PlayerData data, IPlayerView view)
    {
        base.Setup(data, view);
        idleState = new(this, machine);
        moveState = new(this, machine);
        jumpState = new(this, machine);
        fallState = new(this, machine);
        dashState = new(this, machine);
        ladderState = new(this, machine);
        machine.Init(idleState);
        BindInputAction(Managers.Config.actMap);
        Observable.EveryUpdate(UnityFrameProvider.FixedUpdate).Subscribe(_ =>
        {
            machine.curState.FixedUpdate();
        }).AddTo(this);
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 currentMoveInput = context.ReadValue<Vector2>();

        if (currentMoveInput == Vector2.zero)
            return;

        bool isUp = currentMoveInput.y > 0;
        bool isPureUp = isUp && Mathf.Abs(currentMoveInput.x) < Define.Physics.DEADZONE;
        bool isDoubleTapMode = !Managers.Config.value.control.useModifierDash;
        bool isSameDir = Vector2.Dot(currentMoveInput.normalized, lastMoveDirection.normalized) > Define.Physics.BUFFER_TIME;
        bool isQuickEnough = Time.time - lastMoveInputTime <= Define.Physics.TAP_INTERVAL;

        if (!isPureUp && isDoubleTapMode && isSameDir && isQuickEnough)
            OnDashTrigger();

        lookAt = isPureUp ? Vector2.zero : new Vector2(currentMoveInput.x, isUp ? 0 : currentMoveInput.y).normalized;
        lastMoveDirection = currentMoveInput;
        moveInput = currentMoveInput;
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
        lastMoveInputTime = Time.time;
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        int maxJump = tView.jumpCount.CurrentValue;
        int nextJump = currentJumpCount + 1;

        if (nextJump <= maxJump)
        {
            currentJumpCount = (short)nextJump;
            machine.ChangeState(jumpState);
            Jump();
        }
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        if (Managers.Config.value.control.useModifierDash && moveInput != Vector2.zero)
            OnDashTrigger();
    }

    private void OnDashTrigger()
    {
        if (isCoolingDown || tView.dashCount.CurrentValue <= 0 || machine.curState == dashState)
            return;

        if (lookAt.y > Define.Physics.DEADZONE)
            return;

        machine.ChangeState(dashState);
        lastMoveDirection = Vector2.zero;
        DashCharge();
        Dash();
    }

    public void Move() => ExecuteBehavior<MoveBehavior<IMoveData>>(moveInput);
    public void Jump() => ExecuteBehavior<JumpBehavior<IJumpData>>();
    public void Fall() => ExecuteBehavior<FallBehavior<IJumpData>>();
    public void Dash() => ExecuteBehavior<DashBehavior<IDashData>>();
    public void Gravity() => ExecuteBehavior<GravityBehavior<IPhysicsData>>();
    public void Ladder() => ExecuteBehavior<LadderBehavior<ILadderData>>(moveInput);

    private void DashCharge()
    {
        if (tView is not StatModel registry) 
            return;

        short remain = (short)(tView.dashCount.CurrentValue - 1);
        registry.Set(StatType.DASH_COUNT, remain);

        if (remain <= 0 && !isCoolingDown)
        {
            isCoolingDown = true;
            Observable.Timer(TimeSpan.FromSeconds(tView.dashCooltime.CurrentValue)).Subscribe(_ =>
            {
                registry.Set(StatType.DASH_COUNT, config.dashCount);
                isCoolingDown = false;
            }).AddTo(this);
        }
    }

    public void UseLadder() => machine.ChangeState(ladderState);

    private void BindInputAction(InputActionMap map)
    {
        foreach (var action in map.actions)
        {
            switch (action.name)
            {
                case Define.Input.ACTION_MOVE:
                    action.BindInputEvent(OnMovePerformed, OnMoveCanceled, this);
                    break;
                case Define.Input.ACTION_JUMP:
                    action.BindInputEvent(OnJump, this);
                    break;
                case Define.Input.ACTION_DASH:
                    action.BindInputEvent(OnDash, this);
                    break;
            }
        }

        map.Enable();
    }

    public static void BindInputEvent(InputAction action, Action<InputAction.CallbackContext> performed, Component component) => action.OnPerformedAsObservable().Subscribe(performed).AddTo(component);

    public static void BindInputEvent(InputAction action, Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled, Component component)
    {
        action.OnPerformedAsObservable().Subscribe(performed).AddTo(component);
        action.OnCanceledAsObservable().Subscribe(canceled).AddTo(component);
    }
}
