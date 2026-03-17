using UnityEngine;

public abstract class PlayerState<TBehavior> : AgentState<PlayerControl, TBehavior> where TBehavior : class, IAgentBehavior
{
    protected PlayerState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }
}

public class PlayerIdleState : PlayerState<IAgentBehavior>
{
    public PlayerIdleState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override void Enter()
    {
        base.Enter();
        target.isIdling = true;
    }

    public override void FixedUpdate()
    {  
        target.ExecuteMove();
        target.ExecuteFall();
    }

    public override void Exit()
    {
        base.Exit();
        target.isIdling = false;
    }
}

public class PlayerMoveState : PlayerState<MoveBehavior<IMoveData>>
{
    public PlayerMoveState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override void Enter()
    {
        base.Enter();
        target.isMoving = true;
    }

    public override void FixedUpdate()
    {
        target.ExecuteMove();
        target.ExecuteFall();
    }

    public override void Exit() 
    {
        base.Exit();
        target.isMoving = false;
    } 
}

public class PlayerJumpState : PlayerState<JumpBehavior<IJumpData>>
{
    public PlayerJumpState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

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
    }

    public override void Exit() 
    {
        base.Exit();
        target.isJumping = false;
    } 
}

public class PlayerFallState : PlayerState<FallBehavior<IFallData>>
{
    public PlayerFallState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override void Enter()
    {
        base.Enter();
        target.isFalling = true;
    }

    public override void FixedUpdate()
    {
        target.ExecuteMove();
        target.ExecuteFall();
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
    public float elapsed { get; private set; }
    public float duration => behavior.duration;
    public float progress => Mathf.Clamp01(elapsed / duration);

    public PlayerDashState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

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
    }

    public override void Exit()
    {
        base.Exit();
        target.isDashing = false;
    }
}

public class PlayerClimbState : PlayerState<ClimbBehavior<IClimbData>>
{
    public PlayerClimbState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override void Enter()
    {
        base.Enter();
        target.isClimbing = true;
        target.currentJumpCount.Value = 0;

        if (target.currentDashCount.Value > 0) 
            --target.currentDashCount.Value;
    }

    public override void FixedUpdate() => target.ExecuteClimb();

    public override void Exit()
    {
        base.Exit();
        target.isClimbing = false;
    }
}

public class PlayerSneakState : PlayerState<SneakBehavior<ISneakData>>
{
    public PlayerSneakState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

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
