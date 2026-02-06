using UnityEngine;

public class SneakBehavior<T> : IAgentBehavior<T> where T : class, ISneakData
{
    private IAgentControl agent;
    private T config;
    private Vector2 originSize;
    private Vector2 originOffset;

    public void Setup(IAgentControl control, T data)
    {
        agent = control;
        config = data;
        originSize = agent.tCollider.size;
        originOffset = agent.tCollider.offset;
    }

    public void Prepare(BehaviorContext context)
    {
        if (!agent.isGrounded) 
            return;
        agent.tBody.linearVelocity = Vector2.zero;
        var collider = agent.tCollider;
        collider.direction = CapsuleDirection2D.Horizontal;
        float sneakHeight = originSize.y * config.threshold;
        float sneakWidth = originSize.x;
        float heightDifference = originSize.y - sneakHeight;
        float newOffsetY = originOffset.y - (heightDifference * Define.Physics.HALF);
        collider.size = new(sneakWidth, sneakHeight);
        collider.offset = new(originOffset.x, newOffsetY);
    }

    public void Execute(BehaviorContext context = default)
    {
        if (agent is IClimb { isClimbing: false } && agent.isGrounded)
            agent.tBody.linearVelocity = Vector2.zero;
    }

    public void Terminate(BehaviorContext context)
    {
        var collider = agent.tCollider;
        collider.direction = CapsuleDirection2D.Vertical;
        collider.size = originSize;
        collider.offset = originOffset;
    }
}
