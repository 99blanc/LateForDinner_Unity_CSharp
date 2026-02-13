using R3;
using System;

public abstract class State
{
    public virtual int hash => 0;
    public virtual void Enter() { }
    public virtual void FixedUpdate() { }
    public virtual void Exit() { }
    public virtual void HandleState<TInput>(TInput input) { }
}

public abstract class AgentState<TControl, TBehavior> : State where TControl : class, IAgentControl where TBehavior : class, IAgentBehavior
{
    protected readonly TControl target;
    protected readonly StateMachine machine;
    protected readonly Subject<object> subject = new();
    protected readonly TBehavior behavior;
    protected DisposableBag bag;

    public AgentState(TControl ctx, StateMachine sm)
    {
        target = ctx;
        machine = sm;
        behavior = ctx.GetBehavior<TBehavior>();
    }

    public override void Enter()
    {
        bag = new();

        if (behavior is not null)
            behavior.Prepare();
    }

    public override void Exit()
    {
        bag.Dispose();

        if (behavior is not null)
            behavior.Terminate();
    }

    public override void HandleState<TInput>(TInput input) => subject.OnNext(input);

    protected void Bind<TInput, TNextState>(Func<TInput, bool> condition) where TNextState : State => subject.OfType<object, TInput>().Where(condition).Subscribe(_ => machine.Change<TNextState>()).AddTo(ref bag);

    protected void Bind<TInput>(Func<TInput, bool> condition, Func<State> key) => subject.OfType<object, TInput>().Where(condition).Subscribe(_ => machine.Change(key())).AddTo(ref bag);
}

public class IdleState<TControl> : AgentState<TControl, IAgentBehavior> where TControl : class, IAgentControl
{
    public IdleState(TControl ctx, StateMachine sm) : base(ctx, sm) { }
}

public class MoveState<TControl> : AgentState<TControl, MoveBehavior<IMoveData>> where TControl : class, IAgentControl
{
    public MoveState(TControl ctx, StateMachine sm) : base(ctx, sm) { }
}

public class JumpState<TControl> : AgentState<TControl, JumpBehavior<IJumpData>> where TControl : class, IAgentControl
{
    public JumpState(TControl ctx, StateMachine sm) : base(ctx, sm) { }
}

public class FallState<TControl> : AgentState<TControl, FallBehavior<IFallData>> where TControl : class, IAgentControl
{
    public FallState(TControl ctx, StateMachine sm) : base(ctx, sm) { }
}

public class DashState<TControl> : AgentState<TControl, DashBehavior<IDashData>> where TControl : class, IAgentControl
{
    public DashState(TControl ctx, StateMachine sm) : base(ctx, sm) { }
}

public class ClimbState<TControl> : AgentState<TControl, ClimbBehavior<IClimbData>> where TControl : class, IAgentControl
{
    public ClimbState(TControl ctx, StateMachine sm) : base(ctx, sm) { }
}

public class SneakState<TControl> : AgentState<TControl, SneakBehavior<ISneakData>> where TControl : class, IAgentControl
{
    public SneakState(TControl ctx, StateMachine sm) : base(ctx, sm) { }
}
