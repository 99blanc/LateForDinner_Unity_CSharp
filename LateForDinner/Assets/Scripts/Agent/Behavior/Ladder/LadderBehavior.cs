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
        if (agent.pProp == null || !agent.pProp.TryGetComponent<Ladder>(out var ladder))
            return;

        float ladderX = agent.pProp.bounds.center.x;
        float nextX = Mathf.SmoothDamp(agent.tBody.position.x, ladderX, ref xVelocity, Define.Physics.SNAP_TIME);
        float moveY = context.input.y * config.moveSpeed * config.decelLadder * Time.fixedDeltaTime;
        agent.tBody.MovePosition(new Vector2(nextX, agent.tBody.position.y + moveY));
        agent.tBody.linearVelocity = Vector2.zero;
    }
}
