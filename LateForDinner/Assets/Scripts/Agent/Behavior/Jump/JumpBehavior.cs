public class JumpBehavior<T> : IAgentBehavior<T> where T : class, IJumpData
{
    private IAgentControl agent;

    public void Setup(IAgentControl control, T data) => agent = control;

    public void Execute(BehaviorContext context) => agent.tBody.linearVelocity = new(agent.tBody.linearVelocity.x, agent.tView.jumpForce.CurrentValue);
}
