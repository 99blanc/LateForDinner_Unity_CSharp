using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override void FixedUpdate()
    {
        target.Move();
        target.Gravity();
        target.Fall();

        if (target.moveInput.x != 0)
            machine.ChangeState(target.moveState);
    }
}

public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override void FixedUpdate()
    {
        target.Move();
        target.Gravity();
        target.Fall();

        if (target.moveInput.x == 0 && Mathf.Abs(target.tBody.linearVelocity.x) < 0.1f)
            machine.ChangeState(target.idleState);

        if (!target.isNearGround && target.tBody.linearVelocity.y < -0.1f)
            machine.ChangeState(target.fallState);
    }
}

public class PlayerJumpState : PlayerState
{
    public PlayerJumpState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override void Enter() => target.Jump();

    public override void FixedUpdate()
    {
        target.Move();
        target.Gravity();
        target.Fall();

        if (target.tBody.linearVelocity.y <= 0)
            machine.ChangeState(target.fallState);
    }
}

public class PlayerFallState : PlayerState
{
    public PlayerFallState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override void FixedUpdate()
    {
        target.Move();
        target.Gravity();
        target.Fall();

        if (target.isNearGround)
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
        float distance = target.tView.dashDistance.CurrentValue;
        elapsed = 0;
        duration = distance / speed;
        direction = Mathf.Sign(target.lookAt.x);
        var behavior = target.GetBehavior<DashBehavior<IDashData>>();
        behavior.Prepare();
    }

    public override void FixedUpdate()
    {
        elapsed += Time.fixedDeltaTime;
        float percent = Mathf.Clamp01(elapsed / duration);
        target.Dash(percent);

        if (target.moveInput.x != 0 && Mathf.Sign(target.moveInput.x) != direction)
        {
            machine.ChangeState(target.isNearGround ? target.moveState : target.fallState);
            return;
        }

        if (percent >= Define.Physics.PERCENTAGE)
            machine.ChangeState(target.isNearGround ? target.idleState : target.fallState);
    }

    public override void Exit() => target.tBody.gravityScale = Define.Physics.PERCENTAGE;
}

public class PlayerLadderState : PlayerState
{
    public PlayerLadderState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override void Enter() => target.Ladder();

    public override void FixedUpdate()
    {
        if (target.actCollider == null)
        {
            machine.ChangeState(target.fallState);
            return;
        }

        target.Fall();
        target.Ladder();

        if (target.isNearGround && target.moveInput.y < -Define.Physics.DEADZONE)
            machine.ChangeState(target.idleState);
    }
}
