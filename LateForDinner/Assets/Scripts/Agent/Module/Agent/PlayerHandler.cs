public interface IHandler
{
    State GetNextState(IAgentControl target, StateMachine machine, InputContext context);
}

public class PlayerHandler : IHandler
{
    public State GetNextState(IAgentControl target, StateMachine machine, InputContext context)
    {
        var player = (PlayerControl)target;

        if (context.canDash && target.GetBehavior<DashBehavior<IDashData>>().CanDash(true, context.moveInput))
            return machine.Get<PlayerDashState>();

        return machine.curState switch
        {
            PlayerIdleState or PlayerMoveState => HandleGround(player, machine, context),
            PlayerJumpState or PlayerFallState => HandleAir(player, machine, context),
            PlayerDashState dash => HandleDash(player, machine, context, dash),
            PlayerClimbState => HandleClimb(player, machine, context),
            _ => machine.curState
        };
    }

    private State HandleGround(PlayerControl target, StateMachine machine, InputContext context)
    {
        if (context.doInteract)
            return target.isPickuping ? machine.Get<PlayerThrowState>() : machine.Get<PlayerPickupState>();

        if (context.doSneak && context.doJump) 
        { 
            target.isTumbling = true; 
            return machine.Get<PlayerFallState>(); 
        }

        if (context.doJump && target.GetBehavior<JumpBehavior<IJumpData>>().CanJump(true)) 
            return machine.Get<PlayerJumpState>();

        if (target.GetBehavior<ClimbBehavior<IClimbData>>().CanClimb(context.moveInput)) 
            return machine.Get<PlayerClimbState>();

        if (!target.isGrounded && target.tBody.linearVelocity.y < Define.Physics.OFFSET) 
            return machine.Get<PlayerFallState>();

        if (context.doMove) 
            return machine.Get<PlayerMoveState>();

        return machine.Get<PlayerIdleState>();
    }

    private State HandleAir(PlayerControl target, StateMachine machine, InputContext context)
    {
        if (target.isGrounded) 
            return context.doMove ? machine.Get<PlayerMoveState>() : machine.Get<PlayerIdleState>();

        if (context.doJump && target.GetBehavior<JumpBehavior<IJumpData>>().CanJump(true)) 
            return machine.Get<PlayerJumpState>();

        if (target.GetBehavior<ClimbBehavior<IClimbData>>().CanClimb(context.moveInput)) 
            return machine.Get<PlayerClimbState>();

        return machine.curState;
    }

    private State HandleDash(PlayerControl target, StateMachine machine, InputContext context, PlayerDashState dash)
    {
        if (dash.elapsed < Define.Physics.BUFFER) 
            return dash;

        if (context.doJump && target.GetBehavior<JumpBehavior<IJumpData>>().CanJump(true))
            return machine.Get<PlayerJumpState>();

        bool isTimeOut = dash.progress >= Define.Physics.FULL;
        bool isOpposite = dash.elapsed > Define.Physics.SNAP && target.IsOppositeInput(context.moveInput.x, dash.behavior.direction);

        if (!isTimeOut && !isOpposite) 
            return dash;

        if (!target.isGrounded) 
            return machine.Get<PlayerFallState>();

        return context.doMove ? machine.Get<PlayerMoveState>() : machine.Get<PlayerIdleState>();
    }

    private State HandleClimb(PlayerControl target, StateMachine machine, InputContext context)
    {
        if (!target.GetBehavior<ClimbBehavior<IClimbData>>().CanClimb(context.moveInput))
            return target.isGrounded ? machine.Get<PlayerIdleState>() : machine.Get<PlayerFallState>();

        if (context.doJump && target.GetBehavior<JumpBehavior<IJumpData>>().CanJump(true)) 
            return machine.Get<PlayerJumpState>();

        return machine.curState;
    }
}
