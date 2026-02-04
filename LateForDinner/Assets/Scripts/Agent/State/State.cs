using UnityEngine;

public abstract class State
{
    public virtual void Enter() { }
    public virtual void FixedUpdate() { }
    public virtual void Exit() { }

    public virtual bool Transition(Vector2 input) => true;
}

public abstract class AgentState<TContext> : State where TContext : class, IAgentControl
{
    protected readonly TContext target;
    protected readonly StateMachine machine;

    public AgentState(TContext ctx, StateMachine sm)
    {
        target = ctx;
        machine = sm;
    }
}

public abstract class AgentState<TContext, TBehavior> : AgentState<TContext> where TContext : class, IAgentControl where TBehavior : class, IAgentBehavior
{
    protected readonly TBehavior behavior;

    public AgentState(TContext ctx, StateMachine sm) : base(ctx, sm) => behavior = ctx.GetBehavior<TBehavior>();

    public override void Enter()
    {
        var context = GetContext();
        behavior.Prepare(context);
    }
    public override void Exit()
    {
        var context = GetContext();
        behavior.Terminate(context);
    }

    protected virtual BehaviorContext GetContext() => BehaviorContext.Default;
}

public class IdleState<T> : AgentState<T> where T : class, IAgentControl
{
    public IdleState(T ctx, StateMachine sm) : base(ctx, sm) { }
}

public class MoveState<T> : AgentState<T, MoveBehavior<IMoveData>> where T : class, IAgentControl
{
    public MoveState(T ctx, StateMachine sm) : base(ctx, sm) { }
}

public class JumpState<T> : AgentState<T, JumpBehavior<IJumpData>> where T : class, IAgentControl
{
    public JumpState(T ctx, StateMachine sm) : base(ctx, sm) { }
}

public class FallState<T> : AgentState<T, FallBehavior<IFallData>> where T : class, IAgentControl
{
    public FallState(T ctx, StateMachine sm) : base(ctx, sm) { }
}

public class DashState<T> : AgentState<T, DashBehavior<IDashData>> where T : class, IAgentControl
{
    public DashState(T ctx, StateMachine sm) : base(ctx, sm) { }
}

public class ClimbState<T> : AgentState<T, ClimbBehavior<IClimbData>> where T : class, IAgentControl
{
    public ClimbState(T ctx, StateMachine sm) : base(ctx, sm) { }
}

public class SneakState<T> : AgentState<T, SneakBehavior<ISneakData>> where T : class, IAgentControl
{
    public SneakState(T ctx, StateMachine sm) : base(ctx, sm) { }
}
