using R3;
using Token.ID;
using Token.PRIORITY;

public class PlayerControl : AgentControl<IPlayerView, PlayerData, PlayerID>, ILadderAgent
{
    public override ModulePrority priority => ModulePrority.PLAYER_CONTROL;
    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    public PlayerJumpState jumpState { get; private set; }
    public PlayerFallState fallState { get; private set; }
    public PlayerDashState dashState { get; private set; }
    public PlayerLadderState ladderState { get; private set; }

    private InputSystem input;

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
        bool isForbidden = isGrounded && ctx.moveInput.y < 0;

        PlayerState target = ctx switch
        {
            { canDash: true } when !isForbidden => dashState,
            { doJump: true } when currentJumpCount < tView.jumpCount.CurrentValue => jumpState,
            { onLadder: true } => ladderState,
            { hasX: true } => moveState,
            _ => isGrounded ? idleState : null
        };

        if (target is null)
            return;

        switch (target)
        {
            case PlayerDashState: input.UseDash(); break;
            case PlayerJumpState: currentJumpCount++; break;
            case PlayerMoveState when isGrounded: currentJumpCount = 0; break;
            case PlayerLadderState: currentJumpCount = 0; break;
        }

        machine.ChangeState(target, target == jumpState);
    }

    public void ExecuteMove() => ExecuteBehavior<MoveBehavior<IMoveData>>(new() { input = moveInput });

    public void ExecuteJump() => ExecuteBehavior<JumpBehavior<IJumpData>>();

    public void ExecuteFall() => ExecuteBehavior<FallBehavior<IJumpData>>();

    public void ExecuteDash(float percent) => ExecuteBehavior<DashBehavior<IDashData>>(new() { bias = percent });

    public void ExecuteGravity() => ExecuteBehavior<GravityBehavior<IPhysicsData>>();

    public void ExecuteLadder() => ExecuteBehavior<LadderBehavior<ILadderData>>(new() { input = moveInput });

    public void UseLadder() => machine.ChangeState(ladderState);
}
