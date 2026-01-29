using UnityEngine;

public abstract class PlayerState
{
    protected PlayerControl ctx;
    protected PlayerStateMachine machine;

    public PlayerState(PlayerControl ctx, PlayerStateMachine sm) { this.ctx = ctx; machine = sm; }

    public virtual void Enter() { }

    public virtual void FixedUpdate() { }

    public virtual void HandleJump() { }

    public virtual void HandleDash() { }

    public virtual void Exit() { }
}

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerControl ctx, PlayerStateMachine sm) : base(ctx, sm) { }
    
    public override void FixedUpdate()
    {
        ctx.ApplyMove();

        if (ctx.moveInput.x != 0) 
            machine.ChangeState(ctx.moveState);
    }
    
    public override void HandleJump() => machine.ChangeState(ctx.jumpState);

    public override void HandleDash() => machine.ChangeState(ctx.dashState);
}

public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(PlayerControl ctx, PlayerStateMachine sm) : base(ctx, sm) { }

    public override void FixedUpdate()
    {
        ctx.ApplyMove();

        if (ctx.moveInput.x == 0 && Mathf.Abs(ctx.rBody.linearVelocity.x) < 0.1f)
            machine.ChangeState(ctx.idleState);

        if (!ctx.isNearGround && ctx.rBody.linearVelocity.y < -0.1f)
            machine.ChangeState(ctx.fallState);
    }

    public override void HandleJump() => machine.ChangeState(ctx.jumpState);

    public override void HandleDash() => machine.ChangeState(ctx.dashState);
}

public class PlayerJumpState : PlayerState
{
    public PlayerJumpState(PlayerControl ctx, PlayerStateMachine sm) : base(ctx, sm) { }

    public override void Enter() => ctx.ApplyJump();

    public override void FixedUpdate()
    {
        ctx.ApplyMove();

        if (ctx.rBody.linearVelocity.y <= 0) 
            machine.ChangeState(ctx.fallState);
    }

    public override void HandleJump() => Enter();

    public override void HandleDash() => machine.ChangeState(ctx.dashState);
}

public class PlayerDashState : PlayerState
{
    public PlayerDashState(PlayerControl ctx, PlayerStateMachine sm) : base(ctx, sm) { }

    public override void Enter() => ctx.ApplyDash();

    public override void FixedUpdate()
    {
        float speedX = Mathf.Abs(ctx.rBody.linearVelocity.x);

        if (speedX < ctx.cView.moveSpeed.CurrentValue * 1.1f)
            Next();
    }

    private void Next()
    {
        if (ctx.isNearGround)
            machine.ChangeState(ctx.idleState);
        else
            machine.ChangeState(ctx.fallState);
    }

    public override void Exit() => ctx.rBody.gravityScale = 1.0f;

    public override void HandleJump() => machine.ChangeState(ctx.jumpState);

    public override void HandleDash() => Enter();
}

public class PlayerFallState : PlayerState
{
    public PlayerFallState(PlayerControl ctx, PlayerStateMachine sm) : base(ctx, sm) { }

    public override void FixedUpdate()
    {
        ctx.ApplyMove();

        if (ctx.isNearGround) 
            machine.ChangeState(ctx.idleState);
    }

    public override void HandleJump() => machine.ChangeState(ctx.jumpState);

    public override void HandleDash() => machine.ChangeState(ctx.dashState);
}