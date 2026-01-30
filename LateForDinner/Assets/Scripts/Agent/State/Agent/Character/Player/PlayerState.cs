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
    public PlayerDashState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    public override void Enter() => target.Dash();

    public override void FixedUpdate()
    {
        target.Move();
        float speedSqr = target.tBody.linearVelocity.sqrMagnitude;
        float limitSqr = Mathf.Pow(target.tView.moveSpeed.CurrentValue * 0.5f, 2);

        if (speedSqr < limitSqr || target.isNearGround)
            machine.ChangeState(target.isNearGround ? target.idleState : target.fallState);
    }

    public override void Exit() => target.tBody.gravityScale = 1.0f;
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

        if (target.isNearGround && target.moveInput.y < -0.1f)
            machine.ChangeState(target.idleState);
    }
}
