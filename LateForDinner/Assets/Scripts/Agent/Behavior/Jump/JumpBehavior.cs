public class JumpBehavior<T> : IAgentBehavior<T> where T : class, IJumpData
{
    private IAgentControl agent;

    public void Setup(IAgentControl control, T data) => agent = control;

    public void Prepare() { }

    public void Execute(BehaviorContext context = default)
    {
        if (agent is not IJump jump)
            return;

        ++jump.currentJumpCount.Value;
        agent.tBody.linearVelocity = new(agent.tBody.linearVelocity.x, agent.tView.jumpForce.CurrentValue);
    }

    public void Terminate() { }

    public bool CanJump(bool jumpRequested)
    {
        if (agent is not IJump jump) 
            return false;

        bool hasCount = jump.currentJumpCount.Value < agent.tView.jumpCount.CurrentValue;
        return jumpRequested && hasCount;
    }
}
