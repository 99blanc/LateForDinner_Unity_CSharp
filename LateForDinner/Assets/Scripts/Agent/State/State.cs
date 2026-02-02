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

public abstract class PlayerState : AgentState<PlayerControl>
{
    public PlayerState(PlayerControl ctx, StateMachine sm) : base(ctx, sm) { }

    protected void ApplyPhysics() 
    { 
        target.ExecuteMove(); 
        target.ExecuteGravity(); 
        target.ExecuteFall();
    }
}