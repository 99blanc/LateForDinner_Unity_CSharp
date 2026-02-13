using R3;
using UnityEngine;

public abstract class PlayerState<TBehavior> : AgentState<PlayerControl, TBehavior> where TBehavior : class, IAgentBehavior
{
    protected PlayerState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }
}

public class PlayerIdleState : PlayerState<IAgentBehavior>
{
    public PlayerIdleState(PlayerControl ctx, StateMachine sm) : base(ctx, sm)
    {
        Bind<InputContext, PlayerFallState>(c => c.doTumble);
        Bind<InputContext, PlayerDashState>(c => c.canDash);
        Bind<InputContext, PlayerJumpState>(c => c.doJump);
        Bind<InputContext, PlayerClimbState>(c => c.doClimb);
        Bind<InputContext, PlayerSneakState>(c => c.doSneak);
        Bind<InputContext, PlayerMoveState>(c => c.doMove);
        Bind<InputContext>(c => c.doInteract, () => target.isPickuping ? machine.Get<PlayerThrowState>() : machine.Get<PlayerPickupState>());
    }

    public override void Enter()
    {
        base.Enter();
        target.isIdling = true;
    }

    public override void FixedUpdate()
    {  
        target.ExecuteMove();
        target.ExecuteFall();

        if (!target.isGrounded) 
            machine.Change<PlayerFallState>();
    }

    public override void Exit()
    {
        base.Exit();
        target.isIdling = false;
    }
}

public class PlayerMoveState : PlayerState<MoveBehavior<IMoveData>>
{
    public PlayerMoveState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) 
    {
        Bind<InputContext, PlayerFallState>(c => c.doTumble);
        Bind<InputContext, PlayerDashState>(c => c.canDash);
        Bind<InputContext, PlayerJumpState>(c => c.doJump);
        Bind<InputContext, PlayerClimbState>(c => c.doClimb);
        Bind<InputContext, PlayerSneakState>(c => c.doSneak);
        Bind<InputContext>(c => !c.doMove, () => machine.Get<PlayerIdleState>());
        Bind<InputContext>(c => c.doInteract, () => target.isPickuping ? machine.Get<PlayerThrowState>() : machine.Get<PlayerPickupState>());
    }

    public override void Enter()
    {
        base.Enter();
        target.isMoving = true;
    }

    public override void FixedUpdate()
    {
        target.ExecuteMove();
        target.ExecuteFall();

        if (!target.isGrounded) 
            machine.Change<PlayerFallState>();
    }

    public override void Exit() 
    {
        base.Exit();
        target.isMoving = false;
    } 
}

public class PlayerJumpState : PlayerState<JumpBehavior<IJumpData>>
{
    public PlayerJumpState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) 
    {
        Bind<InputContext, PlayerClimbState>(c => c.doClimb);
        Bind<InputContext, PlayerDashState>(c => c.canDash);
        subject.OfType<object, InputContext>().Where(c => c.doJump).Subscribe(_ => target.ExecuteJump()).AddTo(ref bag);
    }

    public override void Enter()
    {
        base.Enter();
        target.isJumping = true;
        target.ExecuteJump();
    }

    public override void FixedUpdate()
    {
        target.ExecuteMove();
        target.ExecuteFall();

        if (target.tBody.linearVelocity.y < -Define.Physics.TICK)
            machine.Change<PlayerFallState>();
    }

    public override void Exit() 
    {
        base.Exit();
        target.isJumping = false;
    } 
}

public class PlayerFallState : PlayerState<FallBehavior<IFallData>>
{
    public PlayerFallState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) 
    {
        Bind<InputContext>(c => !target.isTumbling && c.canDash, () => machine.Get<PlayerDashState>());
        Bind<InputContext>(c => !target.isTumbling && c.doJump, () => machine.Get<PlayerJumpState>());
        Bind<InputContext>(c => !target.isTumbling && c.doClimb, () => machine.Get<PlayerClimbState>());
    }

    public override void Enter()
    {
        base.Enter();
        target.isFalling = true;
    }

    public override void FixedUpdate()
    {
        target.ExecuteMove();
        target.ExecuteFall();

        if (target.isGrounded) 
            machine.Change(target.moveInput.x != 0 ? machine.Get<PlayerMoveState>() : machine.Get<PlayerIdleState>());
    }

    public override void Exit()
    {
        base.Exit();
        target.isFalling = false;
        target.isTumbling = false; 
    }
}

public class PlayerDashState : PlayerState<DashBehavior<IDashData>>
{
    private float elapsed;

    public PlayerDashState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) 
    {
        Bind<InputContext, PlayerClimbState>(c => c.doClimb);
        Bind<InputContext, PlayerJumpState>(c => c.doJump);
    }

    public override void Enter()
    {
        base.Enter();
        target.isDashing = true;
        elapsed = 0;
    }

    public override void FixedUpdate()
    {
        elapsed += Time.fixedDeltaTime;
        float percent = Mathf.Clamp01(elapsed / behavior.duration);
        target.ExecuteDash(percent);
        bool isOpposite = elapsed > Define.Physics.SNAP && target.IsOppositeInput(target.moveInput.x, behavior.direction);

        if (percent >= Define.Physics.FULL || isOpposite)
            machine.Change(target.isGrounded ? (target.moveInput.x != 0 ? machine.Get<PlayerMoveState>() : machine.Get<PlayerIdleState>()) : machine.Get<PlayerFallState>());

    }

    public override void Exit()
    {
        base.Exit();
        target.isDashing = false;
    }
}

public class PlayerClimbState : PlayerState<ClimbBehavior<IClimbData>>
{
    public PlayerClimbState(PlayerControl ctx, StateMachine sm) : base(ctx, sm)
    {
        Bind<InputContext, PlayerJumpState>(c => c.doJump);
        Bind<InputContext, PlayerDashState>(c => c.canDash);
    }

    public override void Enter()
    {
        base.Enter();
        target.isClimbing = true;
        target.currentJumpCount.Value = 0;

        if (target.currentDashCount.Value > 0) 
            --target.currentDashCount.Value;
    }

    public override void FixedUpdate()
    {
        target.ExecuteClimb();

        if (!behavior.CanClimb(target.moveInput))
            machine.Change(target.isGrounded ? (target.moveInput.y < 0 ? machine.Get<PlayerSneakState>() : target.moveInput.x != 0 ? machine.Get<PlayerMoveState>() : machine.Get<PlayerIdleState>()) : machine.Get<PlayerFallState>());
    }

    public override void Exit()
    {
        base.Exit();
        target.isClimbing = false;
    }
}

public class PlayerSneakState : PlayerState<SneakBehavior<ISneakData>>
{
    public PlayerSneakState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) 
    {
        Bind<InputContext, PlayerFallState>(c => !target.isGrounded);
        Bind<InputContext>(c => !c.doSneak, () => target.moveInput.x != 0 ? machine.Get<PlayerMoveState>() : machine.Get<PlayerIdleState>());
    }

    public override void Enter()
    {
        base.Enter();
        target.isSneaking = true;
        target.ExecuteSneak();
    }

    public override void FixedUpdate()
    {
        target.ExecuteFall();
    }

    public override void Exit()
    {
        base.Exit();
        target.isSneaking = false;
    }
}

public class PlayerPickupState : PlayerState<PickupBehavior<IPickupData>>
{
    public PlayerPickupState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override void Enter()
    {
        base.Enter();
        target.isPickuping = true;
        target.ExecutePickup();
        machine.Change(target.isGrounded ? (target.moveInput.x != 0 ? machine.Get<PlayerMoveState>() : machine.Get<PlayerIdleState>()) : machine.Get<PlayerFallState>());
    }
}

public class PlayerThrowState : PlayerState<ThrowBehavior<IThrowData>>
{
    public PlayerThrowState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override void Enter()
    {
        base.Enter(); 
        target.isPickuping = false; 
        target.isThrowing = true; 
        target.ExecuteThrow();
        machine.Change(target.isGrounded ? (target.moveInput.x != 0 ? machine.Get<PlayerMoveState>() : machine.Get<PlayerIdleState>()) : machine.Get<PlayerFallState>());
    }

    public override void Exit()
    {
        base.Exit();
        target.isThrowing = false;
    }
}
