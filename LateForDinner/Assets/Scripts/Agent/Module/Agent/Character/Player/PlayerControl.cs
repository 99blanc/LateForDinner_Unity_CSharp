using Cysharp.Threading.Tasks;
using R3;
using Token.ID;
using Token.PRIORITY;
using UnityEngine;

public class PlayerControl : AgentControl<IPlayerView, PlayerData, PlayerID>, IMove, IJump, IFall, IDash, IClimb, ISneak, ITumble
{
    public override ModulePrority priority => ModulePrority.PLAYER_CONTROL;
    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    public PlayerJumpState jumpState { get; private set; }
    public PlayerFallState fallState { get; private set; }
    public PlayerDashState dashState { get; private set; }
    public PlayerClimbState climbState { get; private set; }
    public PlayerSneakState sneakState { get; private set; }
    public override bool isGrounded => !isTumbling && this.IsGrounded();
    public bool isMoving { get; set; }
    public bool isJumping { get; set; }
    public bool isFalling { get; set; }
    public bool isDashing { get; set; }
    public bool isClimbing { get; set; }
    public bool isSneaking { get; set; }
    public bool isTumbling { get; set; }
    public ReactiveProperty<short> currentJumpCount { get; set; } = new();
    public ReactiveProperty<short> currentDashCount { get; set; } = new();
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

    public override async UniTask Setup(PlayerData data, IPlayerView view, StateMachine machine)
    {
        await base.Setup(data, view, machine);
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
        idleState = new(this, sMachine);
        moveState = new(this, sMachine);
        jumpState = new(this, sMachine);
        fallState = new(this, sMachine);
        dashState = new(this, sMachine);
        climbState = new(this, sMachine);
        sneakState = new(this, sMachine);
        sMachine.Init(idleState);
        input = new(this);
        input.Init();
        Observable.EveryUpdate(UnityFrameProvider.FixedUpdate).Subscribe(_ =>
        {
            sMachine.curState.FixedUpdate();
        }).AddTo(this);
    }

    public void HandleInput(InputContext ctx)
    {
        moveInput = ctx.moveInput;
        State target = ctx switch
        {
            { doTumble: true } when (isTumbling = true) is var _ => fallState,
            { canDash: true } => dashState,
            { doJump: true } => jumpState,
            { doClimb: true } => climbState,
            { doSneak: true } => sneakState,
            { doMove: true } => moveState,
            _ => idleState
        };

        if (!isGrounded && (target == moveState || target == idleState))
            target = (tBody.linearVelocity.y > 0) ? jumpState : fallState;

        if (sMachine.curState == target && !ctx.doJump && !ctx.canDash)
            return;

        bool force = ctx.doTumble || isTumbling || target == dashState || target == jumpState || target == climbState;
        sMachine.Change(target, moveInput, force);
    }

    public void ExecuteMove() => ExecuteBehavior<MoveBehavior<IMoveData>>(new() { input = moveInput });

    public void ExecuteJump() => ExecuteBehavior<JumpBehavior<IJumpData>>();

    public void ExecuteFall() => ExecuteBehavior<FallBehavior<IFallData>>(new() { scala = isTumbling ? 1f : 0 });

    public void ExecuteDash(float percent) => ExecuteBehavior<DashBehavior<IDashData>>(new() { scala = percent });

    public void ExecuteClimb() => ExecuteBehavior<ClimbBehavior<IClimbData>>(new() { input = moveInput });

    public void ExecuteSneak() => ExecuteBehavior<SneakBehavior<ISneakData>>();
}
