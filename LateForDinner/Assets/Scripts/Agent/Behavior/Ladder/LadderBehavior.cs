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

    public void Execute(Vector2 input = default)
    {
        var ladder = agent.actCollider;

        if (ladder is null)
            return;

        agent.tBody.gravityScale = 0;

        if (Mathf.Abs(agent.tBody.position.x - ladder.bounds.center.x) > 0.02f)
        {
            var targetPos = new Vector2(ladder.transform.position.x, agent.tBody.position.y);
            agent.tBody.MovePosition(targetPos);
        }

        float verticalSpeed = input.y * agent.tView.moveSpeed.CurrentValue;
        float horizontalSpeed = input.x * agent.tView.moveSpeed.CurrentValue * config.decelLadder;
        agent.tBody.linearVelocity = new Vector2(horizontalSpeed, verticalSpeed);
    }
}
