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

    public void Execute(BehaviorContext context)
    {
        bool isClimbing = agent is IClimb && agent.pProp is IClimbProp;
        Vector2 bottom = new(agent.tCollider.bounds.center.x, agent.tCollider.bounds.min.y + Define.Physics.OFFSET);
        Vector2 castSize = new(agent.tCollider.bounds.size.x * 0.9f, Define.Physics.OFFSET);
        RaycastHit2D hit = Physics2D.BoxCast(bottom, castSize, 0, Vector2.down, config.gcDistance, Define.Layer.GROUND_MASKS);
        bool detectedGrounded = hit.collider is not null && !hit.collider.isTrigger && agent.tBody.linearVelocity.y < Define.Physics.OFFSET;
        agent.isGrounded = detectedGrounded && !isClimbing;
        agent.isFalling = !agent.isGrounded && agent.tBody.linearVelocity.y < -config.threshold;
        agent.currentJumpCount = agent.isGrounded ? (short)0 : agent.currentJumpCount;
    }
}
