using UnityEngine;

public abstract class PlayerState
{
    protected PlayerControl ctx;
    protected PlayerStateMachine machine;

    public PlayerState(PlayerControl ctx, PlayerStateMachine sm) { this.ctx = ctx; machine = sm; }

    public virtual void Enter() { }

    public virtual void FixedUpdate() { }

    public virtual void HandleJump() { }

    public virtual void Exit() { }
}

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerControl ctx, PlayerStateMachine sm) : base(ctx, sm) { }
    
    public override void FixedUpdate()
    {
        ctx.ApplyMovement();

        if (ctx.moveInput.x != 0) 
            machine.ChangeState(ctx.MoveState);
    }
    
    public override void HandleJump() => machine.ChangeState(ctx.JumpState);
}

public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(PlayerControl ctx, PlayerStateMachine sm) : base(ctx, sm) { }

    public override void FixedUpdate()
    {
        ctx.ApplyMovement();

        if (ctx.moveInput.x == 0 && Mathf.Abs(ctx.rBody.linearVelocity.x) < 0.1f)
            machine.ChangeState(ctx.IdleState);

        if (!ctx.isNearGround && ctx.rBody.linearVelocity.y < -0.1f)
            machine.ChangeState(ctx.FallState);
    }

    public override void HandleJump() => machine.ChangeState(ctx.JumpState);
}

public class PlayerJumpState : PlayerState
{
    public PlayerJumpState(PlayerControl ctx, PlayerStateMachine sm) : base(ctx, sm) { }

    public override void Enter()
    {
        int maxJump = ctx.cView.jumpCount.CurrentValue;
        int nextJump = ctx.isNearGround ? 1 : ctx.currentJumpCount + 1;

        if (nextJump <= maxJump)
        {
            ctx.currentJumpCount = (short)nextJump;
            ctx.rBody.linearVelocity = new Vector2(ctx.rBody.linearVelocity.x, ctx.cView.jumpForce.CurrentValue);
            ctx.isNearGround = false;
        }
    }

    public override void FixedUpdate()
    {
        ctx.ApplyMovement();

        if (ctx.rBody.linearVelocity.y <= 0) 
            machine.ChangeState(ctx.FallState);
    }

    public override void HandleJump() => Enter();
}

public class PlayerFallState : PlayerState
{
    public PlayerFallState(PlayerControl ctx, PlayerStateMachine sm) : base(ctx, sm) { }

    public override void FixedUpdate()
    {
        ctx.ApplyMovement();

        if (ctx.isNearGround) 
            machine.ChangeState(ctx.IdleState);
    }

    public override void HandleJump() => machine.ChangeState(ctx.JumpState);
}