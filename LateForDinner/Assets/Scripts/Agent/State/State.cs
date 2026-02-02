public abstract class State
{
    public virtual void Enter() { }
    public virtual void FixedUpdate() { }
    public virtual void Exit() { }
}

public abstract class AgentState<T> : State where T : class, IAgentControl
{
    protected T target;
    protected StateMachine machine;

    public AgentState(T ctx, StateMachine sm) { target = ctx; machine = sm; }
}

public class IdleState<T> : AgentState<T> where T : class, IAgentControl
{
    public IdleState(T ctx, StateMachine sm) : base(ctx, sm) { }
}

public class MoveState<T> : AgentState<T> where T : class, IAgentControl
{
    public MoveState(T ctx, StateMachine sm) : base(ctx, sm) { }
}

public class JumpState<T> : AgentState<T> where T : class, IAgentControl
{
    public JumpState(T ctx, StateMachine sm) : base(ctx, sm) { }
}

public class FallState<T> : AgentState<T> where T : class, IAgentControl
{
    public FallState(T ctx, StateMachine sm) : base(ctx, sm) { }
}

public class DashState<T> : AgentState<T> where T : class, IAgentControl
{
    public DashState(T ctx, StateMachine sm) : base(ctx, sm) { }
}

public class ClimbState<T> : AgentState<T> where T : class, IAgentControl
{
    public ClimbState(T ctx, StateMachine sm) : base(ctx, sm) { }
}

public class SneakState<T> : AgentState<T> where T : class, IAgentControl
{
    public SneakState(T ctx, StateMachine sm) : base(ctx, sm) { }
}
