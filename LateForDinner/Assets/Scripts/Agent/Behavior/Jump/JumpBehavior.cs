using UnityEngine;

public class JumpBehavior<T> : IAgentBehavior<T> where T : class, IJumpData
{
    private IAgentControl agent;
    private T config;

    public void Setup(IAgentControl control, T data)
    {
        agent = control;
        config = data;
    }

    public void Execute(Vector2 input = default) => agent.tBody.linearVelocity = new Vector2(agent.tBody.linearVelocity.x, agent.tView.jumpForce.CurrentValue);
}
