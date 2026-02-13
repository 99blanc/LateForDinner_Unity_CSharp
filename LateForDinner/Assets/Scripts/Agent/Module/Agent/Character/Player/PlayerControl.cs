using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using Token.ID;
using Token.PRIORITY;

public class PlayerControl : AgentControl<IPlayerView, PlayerData, PlayerID, InputContext>, IMove, IJump, IFall, IDash, IClimb, ISneak, ITumble, IPickup, IThrow
{
    public override ModulePrority priority => ModulePrority.PLAYER_CONTROL;
    public override bool isGrounded => !isTumbling && this.IsGrounded();
    public bool isMoving { get; set; }
    public bool isJumping { get; set; }
    public bool isFalling { get; set; }
    public bool isDashing { get; set; }
    public bool isClimbing { get; set; }
    public bool isSneaking { get; set; }
    public bool isTumbling { get; set; }
    public bool isPickuping { get; set; }
    public bool isThrowing { get; set; }
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
        SetBehavior(new PickupBehavior<IPickupData>());
        SetBehavior(new ThrowBehavior<IThrowData>());
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
        sMachine.Setup
        (
            new PlayerIdleState(this, sMachine),
            new PlayerMoveState(this, sMachine),
            new PlayerJumpState(this, sMachine),
            new PlayerFallState(this, sMachine),
            new PlayerDashState(this, sMachine),
            new PlayerClimbState(this, sMachine),
            new PlayerSneakState(this, sMachine),
            new PlayerPickupState(this, sMachine),
            new PlayerThrowState(this, sMachine)
        );
        sMachine.Init<PlayerIdleState>();
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
        intent = ctx;
        sMachine.curState.HandleState(ctx);
    }

    public void ExecuteMove() => ExecuteBehavior<MoveBehavior<IMoveData>>(new() { input = moveInput });

    public void ExecuteJump() => ExecuteBehavior<JumpBehavior<IJumpData>>();

    public void ExecuteFall() => ExecuteBehavior<FallBehavior<IFallData>>(new() { scala = isTumbling ? 1f : 0 });

    public void ExecuteDash(float percent) => ExecuteBehavior<DashBehavior<IDashData>>(new() { scala = percent });

    public void ExecuteClimb() => ExecuteBehavior<ClimbBehavior<IClimbData>>(new() { input = moveInput });

    public void ExecuteSneak() => ExecuteBehavior<SneakBehavior<ISneakData>>();

    public void ExecutePickup() => ExecuteBehavior<PickupBehavior<IPickupData>>();

    public void ExecuteThrow() => ExecuteBehavior<ThrowBehavior<IThrowData>>(new() { input = moveInput });
}
