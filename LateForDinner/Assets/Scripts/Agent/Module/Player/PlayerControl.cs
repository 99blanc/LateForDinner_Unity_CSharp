using R3;
using R3.Triggers;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Token.ID;
using Token.PRIORITY;

public class PlayerControl : MonoBehaviour, IAgentModule<IPlayerView, PlayerData, PlayerID>
{
    private PlayerStateMachine machine;
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerFallState FallState { get; private set; }

    public PlayerData pData { get; private set; }
    public IActionView cView { get; private set; }
    public Rigidbody2D rBody { get; private set; }
    public CapsuleCollider2D cCollider { get; private set; }

    public Vector2 moveInput { get; set; }
    public short currentJumpCount { get; set; }
    public bool isNearGround { get; set; }
    public ModulePrority priority => ModulePrority.PLAYER_CONTROL;

    public void Setup(PlayerData data, IPlayerView view)
    {
        pData = data;
        cView = view;
        rBody = gameObject.GetComponentAssert<Rigidbody2D>();
        cCollider = gameObject.GetComponentAssert<CapsuleCollider2D>();
        machine = new PlayerStateMachine();
        IdleState = new PlayerIdleState(this, machine);
        MoveState = new PlayerMoveState(this, machine);
        JumpState = new PlayerJumpState(this, machine);
        FallState = new PlayerFallState(this, machine);
        machine.Init(IdleState);
        BindInputAction(Managers.Config.actMap);

        this.FixedUpdateAsObservable().Subscribe(this, (x, state) => 
        {
            CheckGround();
            ApplyFallGravity();
            machine.curState.FixedUpdate();
        }).AddTo(this);
    }

    private void OnMovePerformed(InputAction.CallbackContext context) => moveInput = context.ReadValue<Vector2>();

    private void OnMoveCanceled(InputAction.CallbackContext context) => moveInput = Vector2.zero;

    private void OnJump(InputAction.CallbackContext context) => machine.curState.HandleJump();

    public void ApplyMovement()
    {
        float targetSpeed = moveInput.x * cView.moveSpeed.CurrentValue;
        float currentXVelocity = rBody.linearVelocity.x;
        float isMoving = Mathf.Ceil(Mathf.Abs(moveInput.x));
        float lerpTime = (isMoving * pData.acceleration) + ((1 - isMoving) * pData.deceleration);
        float newXVelocity = Mathf.Lerp(currentXVelocity, targetSpeed, Time.fixedDeltaTime * lerpTime);
        rBody.linearVelocity = new Vector2(newXVelocity, rBody.linearVelocity.y);
        float multiplier = Mathf.Abs(Mathf.Sign(moveInput.x));
        float newScaleX = (multiplier * Mathf.Sign(moveInput.x)) + ((1 - multiplier) * transform.localScale.x);
        transform.localScale = new Vector3(newScaleX, 1, 1);
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
    }

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
