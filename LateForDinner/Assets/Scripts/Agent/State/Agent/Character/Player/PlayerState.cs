using UnityEngine;

public class PlayerIdleState : IdleState<PlayerControl>
{
    public PlayerIdleState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override bool Transition(Vector2 input) => target.isGrounded;

    public override void Enter() => target.isIdling = true;

    public override void FixedUpdate()
    {
        target.ExecuteMove();
        target.ExecuteFall();
    }

    public override void Exit() => target.isIdling = false;
}

public class PlayerMoveState : MoveState<PlayerControl>
{
    public PlayerMoveState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override bool Transition(Vector2 input)
    {
        if (!target.isGrounded) 
            return false;

        return input.x == 0;
    }

    public override void Enter() => target.isMoving = true;

    public override void FixedUpdate()
    {
        target.ExecuteMove();
        target.ExecuteFall();
    }

    public override void Exit() => target.isMoving = false;
}

public class PlayerJumpState : JumpState<PlayerControl>
{
    public PlayerJumpState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override bool Transition(Vector2 input)
    {
        bool canDash = target.GetBehavior<DashBehavior<IDashData>>().CanDash(target.currentDashCount.Value < target.tView.dashCount.CurrentValue, input);
        bool canClimb = target.GetBehavior<ClimbBehavior<IClimbData>>().CanClimb(input);
        return target.isGrounded || canDash || canClimb;
    }

    public override void Enter()
    {
        target.ExecuteJump();
        target.isJumping = true;
    }

    public override void FixedUpdate()
    {
        target.ExecuteMove();
        target.ExecuteFall();

        if (target.isFalling)
        {
            machine.Change(target.fallState, target.moveInput);
            return;
        }

        if (target.isGrounded)
            machine.Change(target.moveInput.x != 0 ? target.moveState : target.idleState);
    }

    public override void Exit() => target.isJumping = false;
}

public class PlayerFallState : FallState<PlayerControl>
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
        bool groundCheck = target.isGrounded;
        bool velocityCheck = target.tBody.linearVelocity.y > -Define.Physics.OFFSET;

        if (target.isGrounded)
        {
            machine.Change(target.moveInput.x != 0 ? target.moveState : target.idleState, target.moveInput, true);
            return;
        }

        if (target.isTumbling && target.isFalling && (target.isGrounded || target.tBody.linearVelocity.y > -Define.Physics.OFFSET))
            target.isTumbling = false;
    }

    public override void Exit() 
    {
        base.Exit();
        target.isFalling = false;
    }
}

public class PlayerDashState : DashState<PlayerControl>
{
    private float elapsed;

    public PlayerDashState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override bool Transition(Vector2 input)
    {
        bool isOpposite = target.IsOppositeInput(input.x, behavior.direction);
        bool canClimb = target.GetBehavior<ClimbBehavior<IClimbData>>().CanClimb(input);
        return isOpposite || canClimb;
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

        if (behavior.IsFinished(percent) || target.IsOppositeInput(target.moveInput.x, behavior.direction))
            machine.Change(target.isGrounded ? (target.moveInput.x != 0 ? target.moveState : target.idleState) : target.fallState, target.moveInput, true);
    }

    public override void Exit()
    {
        base.Exit();
        target.isDashing = false;
    }
}

public class PlayerClimbState : ClimbState<PlayerControl>
{
    public PlayerClimbState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override bool Transition(Vector2 input) => !behavior.CanClimb(input);

    public override void Enter()
    {
        base.Enter();

        if (target is IJump jump)
            jump.currentJumpCount.Value = 0;

        if (target is IDash dash && dash.currentDashCount.Value > 0)
            --dash.currentDashCount.Value;

        target.isClimbing = true;
    }

    public override void FixedUpdate()
    {
        target.ExecuteClimb();

        if (!behavior.CanClimb(target.moveInput))
            machine.Change(target.isGrounded ? (target.moveInput.x != 0 ? target.moveState : target.idleState) : target.fallState, target.moveInput, true);
    }

    public override void Exit()
    {
        base.Exit();
        target.isClimbing = false;
    }
}

public class PlayerSneakState : SneakState<PlayerControl>
{
    public PlayerSneakState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override void Enter()
    {
        base.Enter();
        target.isSneaking = true;
    }

    public override void FixedUpdate()
    {
        if (!target.isGrounded)
            machine.Change(target.isGrounded ? (target.moveInput.x != 0 ? target.moveState : target.idleState) : target.fallState, target.moveInput, true);
    }

    public override void Exit()
    {
        base.Exit();
        target.isSneaking = false;
    }
}
