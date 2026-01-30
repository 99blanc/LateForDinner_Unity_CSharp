using UnityEngine;

public class DashBehavior<T> : IAgentBehavior<T> where T : class, IDashData
{
    private IAgentControl agent;
    private T config;
    private Vector2 startPos;
    private Vector2 targetPos;

    public void Setup(IAgentControl control, T data)
    {
        agent = control;
        config = data;
    }

    public void Prepare()
    {
        int isDown = (agent.lookAt.y < -Define.Physics.DEADZONE) ? 1 : 0;
        Vector2 dashDir = new Vector2(Mathf.Sign(agent.lookAt.x) * (1 - isDown), -1.0f * isDown);
        startPos = agent.tBody.position;
        targetPos = startPos + (dashDir * agent.tView.dashDistance.CurrentValue);
        agent.tBody.gravityScale = 0;
        agent.tBody.linearVelocity = Vector2.zero;
    }

    public void Execute(BehaviorContext context)
    {
        Vector2 nextPos = Vector2.Lerp(startPos, targetPos, context.bias);
        agent.tBody.MovePosition(nextPos);
    }
}
