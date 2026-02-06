using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystem
{
    private PlayerControl player;
    private InputActionMap map;
    private Vector2 lastDirection;
    private float lastInputTime;
    private Vector2 rawLastInput;

    public InputSystem(PlayerControl control) => player = control;

    public void Init()
    {
        map = Managers.Config.actMap;
        map.BindActionMap(player, CreateContext, ctx => player.HandleInput(ctx));
    }

    private InputContext CreateContext()
    {
        Vector2 input = map.FindAction(Define.Input.ACTION_MOVE).ReadValue<Vector2>();
        bool jumpRequested = map.FindAction(Define.Input.ACTION_JUMP).IsPressed();
        bool dashPressed = map.FindAction(Define.Input.ACTION_DASH).IsPressed();
        bool isJustPressed = input != Vector2.zero && rawLastInput == Vector2.zero;
        bool isDirectionChanged = input != Vector2.zero && input != lastDirection;
        bool isUpdateRequired = isJustPressed || isDirectionChanged;
        bool isTap = isUpdateRequired && input.CheckTap(lastDirection, lastInputTime, Define.Physics.INTERVAL);
        lastInputTime = isUpdateRequired ? Time.time : lastInputTime;
        lastDirection = isUpdateRequired ? input : lastDirection;
        rawLastInput = input;
        bool dashRequested = Managers.Config.value.control.useModifierDash ? dashPressed : isTap;
        bool tumbleRequested = input.y < 0 && jumpRequested;
        return new InputContext
        {
            moveInput = input,
            isTap = dashRequested,
            doMove = input.x != 0,
            doJump = !tumbleRequested && jumpRequested && EvaluateJump(jumpRequested),
            canDash = dashRequested && EvaluateDash(dashRequested, input),
            doClimb = EvaluateClimb(input),
            doSneak = input.y < 0,
            doTumble = tumbleRequested
        };
    }

    private bool EvaluateJump(bool jumpRequested) => player.GetBehavior<JumpBehavior<IJumpData>>().CanJump(jumpRequested);

    private bool EvaluateDash(bool dashRequested, Vector2 input) => player.GetBehavior<DashBehavior<IDashData>>().CanDash(dashRequested, input);

    private bool EvaluateClimb(Vector2 input) => player.GetBehavior<ClimbBehavior<IClimbData>>().CanClimb(input);
}
