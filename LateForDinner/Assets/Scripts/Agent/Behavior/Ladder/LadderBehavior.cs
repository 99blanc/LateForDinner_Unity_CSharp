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

    public void Prepare()
    {
        agent.tBody.gravityScale = 0;
        agent.tBody.linearVelocity = Vector2.zero;
    }

    public void Execute(BehaviorContext context)
    {
        if (agent.pProp is not ILadderProp ladder)
            return;

        agent.tBody.gravityScale = 0;
        agent.tBody.linearVelocity = Vector2.zero;
        BoxCollider2D box = agent.pProp.cCollider;
        float ladderX = ladder.centerX;
        float nextX = Mathf.SmoothDamp(agent.tBody.position.x, ladderX, ref xVelocity, Define.Physics.SNAP);
        float moveY = context.input.y * config.moveSpeed * config.decelLadder * Time.fixedDeltaTime;
        agent.tBody.MovePosition(new(nextX, agent.tBody.position.y + moveY));
        agent.tBody.linearVelocity = Vector2.zero;
    }
}
