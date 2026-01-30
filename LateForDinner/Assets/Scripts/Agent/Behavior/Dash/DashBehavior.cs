using UnityEngine;

public class DashBehavior<T> : IAgentBehavior<T> where T : class, IDashData
{
    private IAgentControl agent;
    private T config;

    public void Setup(IAgentControl control, T data)
    {
        agent = control;
        config = data;
    }

    public void Execute(Vector2 input = default)
    {
        int isDown = (agent.lookAt.y < -Define.Physics.DEADZONE) ? 1 : 0;
        Vector2 dashDir = new Vector2(Mathf.Sign(agent.lookAt.x) * (1 - isDown), -1.0f * isDown);
        agent.tBody.gravityScale = 0;
        agent.tBody.linearVelocity = Vector2.zero;
        float dashPower = agent.tView.dashDistance.CurrentValue * Define.Physics.MULTIPLIER;
        agent.tBody.linearVelocity = dashDir * dashPower;
    }
}
