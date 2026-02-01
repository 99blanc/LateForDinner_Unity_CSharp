using R3;
using Token.ID;
using Token.PRIORITY;
using UnityEngine;

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

        PlayerState targetState = ctx switch
        {
            { canDash: true } => dashState,
            { isLadderAction: true } => ladderState,
            { isMovingX: true } => moveState,
            _ => isGrounded ? idleState : null
        };

        if (targetState is null)
            return;

        switch (targetState)
        {
            case PlayerDashState: input.UseDash(); break;
            case PlayerMoveState: currentJumpCount = isGrounded ? (short)0 : currentJumpCount; break;
            case PlayerLadderState: currentJumpCount = 0; break;
        }

        machine.ChangeState(targetState);
    }

    public void OnJumpRequested()
    {
        if (currentJumpCount >= tView.jumpCount.CurrentValue)
            return;

        ++currentJumpCount;
        machine.ChangeState(jumpState, true);
    }

    public void OnDashRequested(InputContext ctx)
    {
        if (!ctx.canDash) 
            return;

        input.UseDash();
        machine.ChangeState(dashState, false);
    }

    public bool IsOppositeInput(float currentDir) => moveInput.x != 0 && Mathf.Sign(moveInput.x) != currentDir;

    public void Move() => ExecuteBehavior<MoveBehavior<IMoveData>>(new() { input = moveInput });

    public void Jump() => ExecuteBehavior<JumpBehavior<IJumpData>>();

    public void Fall() => ExecuteBehavior<FallBehavior<IJumpData>>();

    public void Dash(float percent) => ExecuteBehavior<DashBehavior<IDashData>>(new() { bias = percent });

    public void Gravity() => ExecuteBehavior<GravityBehavior<IPhysicsData>>();

    public void Ladder() => ExecuteBehavior<LadderBehavior<ILadderData>>(new() { input = moveInput });

    public void EnslaveToLadder() => machine.ChangeState(ladderState);
}
