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
        int isDown = agent.lookAt.y < 0 ? 1 : 0;
        Vector2 dashDir = new(Mathf.Sign(agent.lookAt.x) * (1 - isDown), -Define.Physics.FULL * isDown);
        startPos = agent.tBody.position;
        targetPos = startPos + (dashDir * agent.tView.dashDistance.CurrentValue);
        agent.tBody.gravityScale = 0;
        agent.tBody.linearVelocity = Vector2.zero;
    }

    public void Execute(BehaviorContext context)
    {
        Vector2 nextPos = Vector2.Lerp(startPos, targetPos, context.bias);
        Vector2 currentPos = agent.tBody.position;
        Vector2 direction = (nextPos - currentPos).normalized;
        float distance = Vector2.Distance(currentPos, nextPos);
        Vector2 castSize = agent.tCollider.bounds.size * Define.Physics.HALF;
        RaycastHit2D hit = Physics2D.BoxCast(currentPos, castSize, 0, direction, distance + Define.Physics.DEADZONE, LayerMask.GetMask(Define.Layer.GROUND));
        bool canPass = hit.collider is null || hit.collider.isTrigger;

        if (canPass)
        {
            agent.tBody.MovePosition(nextPos);
            return;
        }

        Vector2 snapPos = currentPos + (direction * (hit.distance - Define.Physics.DEADZONE));
        startPos = snapPos;
        targetPos = snapPos;
        agent.tBody.MovePosition(snapPos);
    }
}
