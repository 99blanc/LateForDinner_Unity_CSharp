using R3;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystem
{
    private PlayerControl player;
    private InputActionMap map;
    private Vector2 lastDirection;
    private float lastInputTime;
    private bool jumpQueued;
    private bool dashQueued;
    private bool interactQueued;

    public InputSystem(PlayerControl control) => player = control;

    public void Init()
    {
        map = Managers.Config.actMap;
        Observable.EveryUpdate().Subscribe(_ =>
        {
            UpdateQueues();
        }).AddTo(player);
    }

    private void UpdateQueues()
    {
        if (map.FindAction(Define.Input.ACTION_JUMP).WasPressedThisFrame())
            jumpQueued = true;

        if (map.FindAction(Define.Input.ACTION_DASH).WasPressedThisFrame())
            dashQueued = true;

        if (map.FindAction(Define.Input.ACTION_INTERACT).WasPressedThisFrame())
            interactQueued = true;
    }

    public InputContext Get()
    {
        Vector2 move = map.FindAction(Define.Input.ACTION_MOVE).ReadValue<Vector2>();
        bool isTap = move.CheckTap(ref lastDirection, ref lastInputTime);
        InputContext context = new()
        {
            moveInput = move,
            doJump = jumpQueued,
            canDash = dashQueued || isTap || (Managers.Config.value.control.useModifierDash && map.FindAction(Define.Input.ACTION_DASH).IsPressed()),
            doMove = move.x != 0,
            doSneak = move.y < 0,
            doInteract = interactQueued
        };

        ResetQueues();
        return context;
    }

    private void ResetQueues() => jumpQueued = dashQueued = interactQueued = false;
}
