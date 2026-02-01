using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override void FixedUpdate()
    {
        ApplyPhysics();

        if (target.moveInput.x != 0) 
            machine.ChangeState(target.moveState);

        if (!target.isGrounded && target.tBody.linearVelocity.y < -Define.Physics.DEADZONE) 
            machine.ChangeState(target.fallState);
    }
}

public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override void FixedUpdate()
    {
        ApplyPhysics();

        if (target.moveInput.x == 0 && Mathf.Abs(target.tBody.linearVelocity.x) < Define.Physics.DEADZONE)
            machine.ChangeState(target.idleState);

        if (!target.isGrounded && target.tBody.linearVelocity.y < -Define.Physics.DEADZONE)
            machine.ChangeState(target.fallState);
    }
}

public class PlayerJumpState : PlayerState
{
    public PlayerJumpState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override void Enter() => target.Jump();

    public override void FixedUpdate()
    {
        ApplyPhysics();

        if (target.tBody.linearVelocity.y < -Define.Physics.TAP_INTERVAL)
            machine.ChangeState(target.fallState);
    }
}

public class PlayerFallState : PlayerState
{
    public PlayerFallState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override void FixedUpdate()
    {
        ApplyPhysics();

        if (target.isGrounded)
            machine.ChangeState(target.idleState);
    }
}

public class PlayerDashState : PlayerState
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
        target.Dash(percent);

        if (target.IsOppositeInput(direction) || percent >= Define.Physics.PERCENTAGE)
            machine.ChangeState(target.isGrounded ? target.idleState : target.fallState);
    }

    public override void Exit() => target.tBody.gravityScale = Define.Physics.PERCENTAGE;
}

public class PlayerLadderState : PlayerState
{
    public PlayerLadderState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override void Enter()
    {
        target.tBody.gravityScale = 0;
        target.currentJumpCount = 0;
        target.tBody.linearVelocity = Vector2.zero;
    }

    public override void FixedUpdate()
    {
        target.Ladder();

        if (!target.pProp || (target.moveInput.y < -Define.Physics.DEADZONE && target.isGrounded))
            machine.ChangeState(target.isGrounded ? target.idleState : target.fallState);
    }

    public override void Exit() => target.tBody.gravityScale = Define.Physics.PERCENTAGE;
}
