using UnityEngine;

public class ClimbBehavior<T> : IAgentBehavior<T> where T : class, IClimbData
{
    private IAgentControl agent;
    private T config;
    private float xVelocity;

    public void Setup(IAgentControl control, T data)
    {
        agent = control;
        config = data;
    }

    public void Prepare()
    {
        agent.tBody.gravityScale = 0;
        agent.tBody.linearVelocity = Vector2.zero;
    }

    public void Execute(BehaviorContext context)
    {
        if (agent.hProp is not IClimbProp ladder || agent is not IClimb climb) 
            return;

        float nextX = Mathf.SmoothDamp(agent.tBody.position.x, ladder.centerX, ref xVelocity, Define.Physics.SNAP);
        float moveY = context.input.y * config.moveSpeed * config.decelObj * Time.fixedDeltaTime;
        agent.tBody.MovePosition(new(nextX, agent.tBody.position.y + moveY));
    }
}
