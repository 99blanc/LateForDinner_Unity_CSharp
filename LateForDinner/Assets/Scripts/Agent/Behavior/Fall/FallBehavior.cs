using UnityEngine;

public class FallBehavior<T> : IAgentBehavior<T> where T : class, IJumpData
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
        var hit = Physics2D.BoxCast(agent.tCollider.bounds.center, agent.tCollider.bounds.size, 0, Vector2.down, config.gcDistance, LayerMask.GetMask(Define.Layer.GROUND));
        var nearHit = Physics2D.Raycast(agent.tBody.position, Vector2.down, config.gcNearDistance, LayerMask.GetMask(Define.Layer.GROUND));
        bool isVelocityStatic = Mathf.Abs(agent.tBody.linearVelocity.y) <= config.threshold;
        bool isGrounded = hit.collider is not null && isVelocityStatic;
        agent.isNearGround = isGrounded || (nearHit.collider is not null && isVelocityStatic);
        agent.currentJumpCount = (short)(agent.currentJumpCount * (isGrounded ? 0 : 1));

        if (isGrounded)
            agent.currentJumpCount = 0;
    }
}
