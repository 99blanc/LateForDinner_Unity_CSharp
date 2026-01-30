using UnityEngine;

public class LadderBehavior<T> : IAgentBehavior<T> where T : class, ILadderData
{
    private IAgentControl agent;
    private T config;

    public void Setup(IAgentControl control, T data)
    {
        agent = control;
        config = data;
    }

    public void Execute(BehaviorContext context)
    {
        var ladder = agent.actCollider;

        if (ladder is null)
            return;

        agent.tBody.gravityScale = 0;

        if (Mathf.Abs(agent.tBody.position.x - ladder.bounds.center.x) > Define.Physics.DEADZONE)
        {
            var targetPos = new Vector2(ladder.transform.position.x, agent.tBody.position.y);
            agent.tBody.MovePosition(targetPos);
        }

        float verticalSpeed = context.input.y * agent.tView.moveSpeed.CurrentValue;
        float horizontalSpeed = context.input.x * agent.tView.moveSpeed.CurrentValue * config.decelLadder;
        agent.tBody.linearVelocity = new Vector2(horizontalSpeed, verticalSpeed);
    }
}
