using R3;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Token.DATA;

public struct InputContext
{
    public Vector2 moveInput;
    public bool isTap;
    public bool onLadder;
    public bool hasX;
    public bool canDash;
    public bool doJump;
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

    private void OnJump(InputAction.CallbackContext context) => player.HandleInput(CreateContext(player.moveInput, false, true));

    private void OnDash(InputAction.CallbackContext context) => player.HandleInput(CreateContext(player.moveInput, true, false));

    private InputContext CreateContext(Vector2 input, bool dashRequested, bool jumpRequested = false)
    {
        return new()
        {
            moveInput = input,
            isTap = dashRequested,
            onLadder = EvaluateLadder(input),
            hasX = input.x != 0,
            canDash = EvaluateDash(input, dashRequested),
            doJump = jumpRequested
        };
    }

    private bool EvaluateLadder(Vector2 input)
    {
        if (player.pProp is not IClimbProp ladder)
            return false;

        float ladderTop = ladder.bounds.max.y;
        float ladderBottom = ladder.bounds.min.y;
        float footY = player.tCollider.bounds.min.y;
        float headY = player.tCollider.bounds.max.y;
        float centerY = player.tCollider.bounds.center.y;
        float midLowerY = (footY + centerY) * Define.Physics.HALF;

        if (input.y > 0 && midLowerY > ladderTop - Define.Physics.OFFSET)
            return false;

        bool canUp = input.y > 0 && headY > ladderBottom;
        bool canDown = input.y < 0 && footY < ladderTop;
        return canUp || canDown;
    }

    private bool EvaluateDash(Vector2 input, bool dashRequested)
    {
        bool statReady = !isCoolingDown && player.tView.dashCount.CurrentValue > 0;
        bool stateReady = player.machine.curState != player.dashState;
        bool isUp = input.y > 0;
        bool isDown = input.y < 0;
        bool isForbidden = (player.isGrounded && isDown) || isUp;
        return dashRequested && statReady && stateReady && !isForbidden;
    }

    public void UseDash()
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
