using R3;
using R3.Triggers;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Token.ID;
using Token.PRIORITY;
using Token.DATA;

public class PlayerControl : MonoBehaviour, IAgentModule<IPlayerView, PlayerData, PlayerID>
{
    public PlayerData pData { get; private set; }
    public IActionView cView { get; private set; }
    public Rigidbody2D rBody { get; private set; }
    public CapsuleCollider2D cCollider { get; private set; }
    public Vector2 moveInput { get; set; }
    public short currentJumpCount { get; set; }
    public bool isNearGround { get; set; }
    public float lookAt { get; private set; } = 1.0f;
    public ModulePrority priority => ModulePrority.PLAYER_CONTROL;
    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    public PlayerJumpState jumpState { get; private set; }
    public PlayerFallState fallState { get; private set; }
    public PlayerDashState dashState { get; private set; }
    private PlayerStateMachine machine;
    private float lastMoveInputTime;
    private float lastGroundedTime;
    private Vector2 lastMoveDirection;
    private bool isCoolingDown;

    public void Setup(PlayerData data, IPlayerView view)
    {
        pData = data;
        cView = view;
        rBody = gameObject.GetComponentAssert<Rigidbody2D>();
        cCollider = gameObject.GetComponentAssert<CapsuleCollider2D>();
        machine = new PlayerStateMachine();
        idleState = new PlayerIdleState(this, machine);
        moveState = new PlayerMoveState(this, machine);
        jumpState = new PlayerJumpState(this, machine);
        fallState = new PlayerFallState(this, machine);
        dashState = new PlayerDashState(this, machine);
        machine.Init(idleState);
        BindInputAction(Managers.Config.actMap);

        this.FixedUpdateAsObservable().Subscribe(this, (x, state) => 
        {
            CheckGround();
            ApplyFallGravity();
            machine.curState.FixedUpdate();
        }).AddTo(this);
    }

    private void OnMovePerform(InputAction.CallbackContext context)
    {
        Vector2 currentMoveInput = context.ReadValue<Vector2>();
        bool isDoubleTapMode = !Managers.Config.value.control.useModifierDash;
        bool hasInput = currentMoveInput != Vector2.zero;
        bool isSameDir = currentMoveInput == lastMoveDirection;
        bool isQuickEnough = (Time.time - lastMoveInputTime) <= Define.Input.INPUT_DOUBLE_TAP_TIME;
        int triggerStack = (isDoubleTapMode && hasInput && isSameDir && isQuickEnough) ? 1 : 0;

        for (int index = 0; index < triggerStack; ++index) 
            OnDashTrigger();

        if (currentMoveInput != Vector2.zero)
        {
            lastMoveDirection = currentMoveInput;
            lookAt = Mathf.Sign(currentMoveInput.x);
        }

        lastMoveInputTime = Time.time;
        moveInput = currentMoveInput;
    }

    private void OnMoveCancel(InputAction.CallbackContext context) => moveInput = Vector2.zero;

    private void OnJump(InputAction.CallbackContext context) => machine.curState.HandleJump();

    private void OnDash(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (Managers.Config.value.control.useModifierDash && moveInput != Vector2.zero)
            OnDashTrigger();
    }

    private void OnDashTrigger()
    {
        if (isCoolingDown || cView.dashCount.CurrentValue <= 0 || machine.curState == dashState)
            return;

        machine.ChangeState(dashState);
        UseDashCharge();
    }

    public void ApplyMove()
    {
        float targetSpeed = moveInput.x * cView.moveSpeed.CurrentValue;
        float speedDif = targetSpeed - rBody.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > Define.Input.INPUT_THRESHOLD) ? pData.acceleration : pData.deceleration;

        if (Mathf.Sign(targetSpeed) != Mathf.Sign(rBody.linearVelocity.x) && Mathf.Abs(targetSpeed) > Define.Input.INPUT_THRESHOLD)
            accelRate *= pData.velPower;

        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, Define.Input.INPUT_BUFFER_TIME) * Mathf.Sign(speedDif);
        rBody.AddForce(movement * Vector2.right, ForceMode2D.Force);
        transform.localScale = new Vector3(lookAt, 1, 1);
    }

    public void ApplyJump()
    {
        bool canCoyoteJump = (Time.time - lastGroundedTime) <= pData.gcDistance;
        int maxJump = cView.jumpCount.CurrentValue;
        int nextJump = (isNearGround || canCoyoteJump) ? 1 : currentJumpCount + 1;

        if (nextJump <= maxJump)
        {
            currentJumpCount = (short)nextJump;
            rBody.linearVelocity = new Vector2(rBody.linearVelocity.x, cView.jumpForce.CurrentValue);
            isNearGround = false;
        }
    }

    private void ApplyFallGravity()
    {
        float multiplier = (rBody.linearVelocity.y < 0) ? (pData.gvMul - 1) : (1.0f - Mathf.Clamp(pData.gvReduction, 0f, 0.9f));
        rBody.linearVelocity += Vector2.up * Physics2D.gravity.y * (multiplier - 1) * Time.fixedDeltaTime;
    }

    private void CheckGround()
    {
        var hit = Physics2D.BoxCast(cCollider.bounds.center, cCollider.bounds.size, 0f, Vector2.down, pData.gcDistance, LayerMask.GetMask(Define.Layer.GROUND));
        var nearHit = Physics2D.Raycast(transform.position, Vector2.down, pData.gcNearDistance, LayerMask.GetMask(Define.Layer.GROUND));
        bool isGrounded = (hit.collider is not null && rBody.linearVelocity.y <= pData.threshold);
        bool isActuallyNear = nearHit.collider is not null;
        currentJumpCount = (short)(currentJumpCount * (isGrounded ? 0 : 1));
        isNearGround = isGrounded || isActuallyNear;

        if (isGrounded)
            lastGroundedTime = Time.time;
    }

    public void ApplyDash()
    {
        rBody.linearVelocity = Vector2.zero;
        float dashDir = lookAt;
        rBody.linearVelocity = new Vector2(dashDir * pData.dashDistance, 0);
        rBody.gravityScale = 0f;
    }

    private void UseDashCharge()
    {
        if (cView is not StatModel registry) 
            return;

        short remain = (short)(cView.dashCount.CurrentValue - 1);
        registry.Set(StatType.DASH_COUNT, remain);

        if (remain <= 0 && !isCoolingDown)
        {
            isCoolingDown = true;
            Observable.Timer(TimeSpan.FromSeconds(cView.dashCooltime.CurrentValue)).Subscribe(_ =>
            {
                registry.Set(StatType.DASH_COUNT, pData.dashCount);
                isCoolingDown = false;
            }).AddTo(this);
        }
    }

    private void BindInputAction(InputActionMap map)
    {
        foreach (var action in map.actions)
        {
            switch (action.name)
            {
                case Define.Input.ACTION_MOVE:
                    action.BindInputEvent(OnMovePerform, OnMoveCancel, this);
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
