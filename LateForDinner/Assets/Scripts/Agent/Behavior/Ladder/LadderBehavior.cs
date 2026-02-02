using UnityEngine;

public class LadderBehavior<T> : IAgentBehavior<T> where T : class, ILadderData
{
    private IAgentControl agent;
    private T config;
    private float xVelocity;

    public void Setup(IAgentControl control, T data)
    {
        agent = control;
        config = data;
    }

    public void Execute(BehaviorContext context)
    {
        if (agent.hProp is not IClimbProp ladder || agent is not IClimb { isClimbing: true })
            return;

        float nextX = Mathf.SmoothDamp(agent.tBody.position.x, ladder.centerX, ref xVelocity, Define.Physics.SNAP);
        float moveY = context.input.y * config.moveSpeed * config.decelObj * Time.fixedDeltaTime;
        agent.tBody.MovePosition(new(nextX, agent.tBody.position.y + moveY));
    }
}
