using R3;
using Token.ID;
using Token.PRIORITY;
using UnityEngine;

public class PlayerControl : AgentControl<IPlayerView, PlayerData, PlayerID>, IMove, IJump, IFall, IDash, IClimb, ISneak
{
    public override ModulePrority priority => ModulePrority.PLAYER_CONTROL;
    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    public PlayerJumpState jumpState { get; private set; }
    public PlayerFallState fallState { get; private set; }
    public PlayerDashState dashState { get; private set; }
    public PlayerClimbState climbState { get; private set; }
    public PlayerSneakState sneakState { get; private set; }
    public bool isMoving { get; set; }
    public bool isJumping { get; set; }
    public bool isFalling { get; set; }
    public bool isGrounded { get; set; }
    public bool isDashing { get; set; }
    public bool isClimbing { get; set; }
    public bool isSneaking { get; set; }
    public short currentJumpCount { get; set; }

    private InputSystem input;

    protected override void Behaviors()
    {
        SetBehavior(new MoveBehavior<IMoveData>());
        SetBehavior(new JumpBehavior<IJumpData>());
        SetBehavior(new FallBehavior<IFallData>());
        SetBehavior(new DashBehavior<IDashData>());
        SetBehavior(new ClimbBehavior<IClimbData>());
        SetBehavior(new SneakBehavior<ISneakData>());
    }

    public override void Setup(PlayerData data, IPlayerView view)
    {
        base.Setup(data, view);
        PhysicsMaterial2D mat = new(Define.Layer.PLAYER)
        {
            friction = 0,
            bounciness = 0
        };
        tCollider.sharedMaterial = mat;
        tBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        tBody.sleepMode = RigidbodySleepMode2D.NeverSleep;
        tBody.interpolation = RigidbodyInterpolation2D.Extrapolate;
        tBody.constraints = RigidbodyConstraints2D.FreezeRotation;
        idleState = new(this, machine);
        moveState = new(this, machine);
        jumpState = new(this, machine);
        fallState = new(this, machine);
        dashState = new(this, machine);
        climbState = new(this, machine);
        sneakState = new(this, machine);
        machine.Init(idleState);
        input = new(this);
        input.Init();
        Observable.EveryUpdate(UnityFrameProvider.FixedUpdate).Subscribe(_ =>
        {
            machine.curState.FixedUpdate();
        }).AddTo(this);
    }

    public void HandleInput(InputContext ctx)
    {
        moveInput = ctx.moveInput;
        lookAt = UpdateLookAt(ctx.moveInput);
        State target = ctx switch
        {
            { canDash: true } when !(isGrounded && ctx.moveInput.y < 0) => dashState,
            { doJump: true } when currentJumpCount < tView.jumpCount.CurrentValue => jumpState,
            { onLadder: true } => climbState,
            _ when isSneaking => sneakState,
            { hasX: true } => moveState,
            _ => isGrounded ? idleState : null
        };
        _ = target != null && ApplyStateEffect(target, ctx);
    }

    private bool ApplyStateEffect(State target, InputContext ctx)
    {
        switch (target)
        {
            case PlayerDashState when !ctx.onLadder: input.UseDash(); break;
            case PlayerJumpState: currentJumpCount++; break;
            case PlayerMoveState or PlayerIdleState or PlayerClimbState: currentJumpCount = 0; break;
        }

        machine.ChangeState(target, target == jumpState);
        return true;
    }

    public void ExecuteMove() => ExecuteBehavior<MoveBehavior<IMoveData>>(new() { input = moveInput });

    public void ExecuteJump() => ExecuteBehavior<JumpBehavior<IJumpData>>();

    public void ExecuteFall() => ExecuteBehavior<FallBehavior<IFallData>>();

    public void ExecuteDash(float percent) => ExecuteBehavior<DashBehavior<IDashData>>(new() { bias = percent });

    public void ExecuteClimb() => ExecuteBehavior<ClimbBehavior<IClimbData>>(new() { input = moveInput * config.decelObj });

    public void ExecuteSneak(float threshold) => ExecuteBehavior<SneakBehavior<ISneakData>>(new() { value = threshold });
}
