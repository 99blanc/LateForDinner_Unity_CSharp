using R3;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Token.DATA;

public struct InputContext
{
    public Vector2 moveInput;
    public bool isDoubleTap;
    public bool isLadderAction;
    public bool isMovingX;
    public bool canDash;
}

public class InputSystem
{
    private PlayerControl player;
    private Vector2 lastMoveDirection;
    private float lastMoveInputTime;
    private bool isCoolingDown;

    public InputSystem(PlayerControl control) => player = control;
    
    public void Init() => Set(Managers.Config.actMap);

    private void Set(InputActionMap map)
    {
        foreach (var action in map.actions)
        {
            switch (action.name)
            {
                case Define.Input.ACTION_MOVE: action.BindInputEvent(OnMovePerformed, OnMoveCanceled, player); break;
                case Define.Input.ACTION_JUMP: action.BindInputEvent(OnJump, player); break;
                case Define.Input.ACTION_DASH: action.BindInputEvent(OnDash, player); break;
            }
        }

        map.Enable();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        player.moveInput = input;
        bool dashRequested = false;

        if (!Managers.Config.value.control.useModifierDash)
            dashRequested = (Time.time - lastMoveInputTime <= Define.Physics.INTERVAL) && Vector2.Dot(input.normalized, lastMoveDirection.normalized) > 0.8f;
        
        InputContext ctx = CreateContext(input, dashRequested);
        player.HandleInput(ctx);
        lastMoveDirection = input;
        lastMoveInputTime = Time.time;
    }

    private void OnMoveCanceled(InputAction.CallbackContext context) => player.HandleInput(CreateContext(Vector2.zero, false));

    private void OnJump(InputAction.CallbackContext context) => player.OnJumpRequested();

    private void OnDash(InputAction.CallbackContext context) => player.OnDashRequested(CreateContext(player.moveInput, true));

    public void UseDash()
    {
        lastMoveDirection = Vector2.zero;
        lastMoveInputTime = 0;
        RefillDash();
    }

    private InputContext CreateContext(Vector2 input, bool dashRequested)
    {
        return new()
        {
            moveInput = input,
            isDoubleTap = dashRequested,
            isLadderAction = EvaluateLadder(input),
            isMovingX = Mathf.Abs(input.x) > Define.Physics.DEADZONE,
            canDash = EvaluateDash(input, dashRequested)
        };
    }

    private bool EvaluateLadder(Vector2 input)
    {
        if (player.pProp is not ILadderProp ladder)
            return false;

        float moveY = input.y;
        float ladderTop = ladder.bounds.max.y;
        float ladderBottom = ladder.bounds.min.y;
        float footY = player.tCollider.bounds.min.y;
        float headY = player.tCollider.bounds.max.y;
        bool canClimbUp = moveY > Define.Physics.DEADZONE && footY < ladderTop - Define.Physics.OFFSET;
        bool canClimbDown = moveY < -Define.Physics.DEADZONE && headY > ladderBottom + Define.Physics.OFFSET;
        return canClimbUp || canClimbDown;
    }

    private bool EvaluateDash(Vector2 input, bool dashRequested)
    {
        bool statReady = !isCoolingDown && player.tView.dashCount.CurrentValue > 0;
        bool stateReady = player.machine.curState != player.dashState;
        bool angleReady = input.y <= Define.Physics.DEADZONE;
        bool isDownDash = input.y < -Define.Physics.DEADZONE;
        bool isOnPlatform = player.pProp is IPlatformProp;
        bool canPassThrough = dashRequested && isDownDash && isOnPlatform;

        if (canPassThrough)
        {
            RestoreDash();
            player.pProp.OnDetach(player);
        }

        return dashRequested && statReady && stateReady && angleReady;
    }

    private void RestoreDash()
    {
        bool hasStat = player.tView is StatModel;

        if (!hasStat) 
            return;

        StatModel registry = (StatModel)player.tView;
        short nextCount = (short)(player.tView.dashCount.CurrentValue + 1);
        registry.Set(StatType.DASH_COUNT, nextCount);
    }

    private void RefillDash()
    {
        if (player.tView is not StatModel registry)
            return;

        short remain = (short)(player.tView.dashCount.CurrentValue - 1);
        registry.Set(StatType.DASH_COUNT, remain);

        if (remain > 0 || isCoolingDown)
            return;

        isCoolingDown = true;
        Observable.Timer(TimeSpan.FromSeconds(player.tView.dashCooltime.CurrentValue)).Subscribe(_ =>
        {
            registry.Set(StatType.DASH_COUNT, player.config.dashCount);
            isCoolingDown = false;
        }).AddTo(player);
    }
}
