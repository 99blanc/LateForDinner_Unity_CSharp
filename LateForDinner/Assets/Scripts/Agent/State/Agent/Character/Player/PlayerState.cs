using UnityEngine;

public class PlayerIdleState : IdleState<PlayerControl>
{
    public PlayerIdleState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override void FixedUpdate()
    {
        target.ExecuteMove();
        target.ExecuteFall();

        if (target.isMoving) 
            machine.ChangeState(target.moveState);

        if (!target.isGrounded && target.isFalling) 
            machine.ChangeState(target.fallState);
    }
}

public class PlayerMoveState : MoveState<PlayerControl>
{
    public PlayerMoveState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override void FixedUpdate()
    {
        target.ExecuteMove();
        target.ExecuteFall();

        if (!target.isMoving)
            machine.ChangeState(target.idleState);

        if (!target.isGrounded && target.isFalling)
            machine.ChangeState(target.fallState);
    }
}

public class PlayerJumpState : JumpState<PlayerControl>
{
    public PlayerJumpState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override void Enter() => target.ExecuteJump();

    public override void FixedUpdate()
    {
        target.ExecuteMove();
        target.ExecuteFall();

        if (target.isFalling)
            machine.ChangeState(target.fallState);
    }
}

public class PlayerFallState : FallState<PlayerControl>
{
    public PlayerFallState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override void FixedUpdate()
    {
        target.ExecuteMove();
        target.ExecuteFall();

        if (target.isGrounded)
            machine.ChangeState(target.idleState);
    }
}

public class PlayerDashState : DashState<PlayerControl>
{
    private float elapsed;
    private float duration;
    private float direction;

    public PlayerDashState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override void Enter()
    {
        float speed = target.tView.moveSpeed.CurrentValue * target.config.dashSpeed;
        duration = target.tView.dashDistance.CurrentValue / speed;
        elapsed = 0;
        direction = Mathf.Sign(target.lookAt.x);
        target.GetBehavior<DashBehavior<IDashData>>().Prepare();
    }

    public override void FixedUpdate()
    {
        elapsed += Time.fixedDeltaTime;
        float percent = Mathf.Clamp01(elapsed / duration);
        target.ExecuteDash(percent);
        IAgentControl agent = target;

        if (agent.IsOppositeInput(target.moveInput.x, direction) || percent >= Define.Physics.FULL)
            machine.ChangeState(target.isGrounded ? target.idleState : target.fallState);
    }

    public override void Exit() => target.tBody.gravityScale = Define.Physics.FULL;
}

public class PlayerClimbState : ClimbState<PlayerControl>
{
    public PlayerClimbState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override void Enter() => target.GetBehavior<ClimbBehavior<IClimbData>>().Prepare();

    public override void FixedUpdate()
    {
        target.ExecuteClimb();

        if (target.hProp is not IClimbProp || target.isGrounded)
            machine.ChangeState(target.isGrounded ? target.idleState : target.fallState);
    }

    public override void Exit() => target.tBody.gravityScale = Define.Physics.FULL;
}

public class PlayerSneakState : SneakState<PlayerControl>
{
    public PlayerSneakState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }
}
