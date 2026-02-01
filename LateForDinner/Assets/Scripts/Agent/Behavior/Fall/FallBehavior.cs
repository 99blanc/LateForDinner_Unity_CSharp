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
        Vector2 bottom = new(agent.tCollider.bounds.center.x, agent.tCollider.bounds.min.y);
        RaycastHit2D hit = Physics2D.BoxCast(bottom, agent.tCollider.bounds.size, 0, Vector2.down, config.gcDistance, LayerMask.GetMask(Define.Layer.GROUND));
        bool isVelocityStatic = Mathf.Abs(agent.tBody.linearVelocity.y) <= config.threshold;
        bool isGrounded = hit.collider is not null && isVelocityStatic;
        bool isFalling = !isGrounded && agent.tBody.linearVelocity.y < -config.threshold;
        agent.isGrounded = isGrounded;
        agent.isFalling = isFalling;
        agent.currentJumpCount = (short)(agent.currentJumpCount * (isGrounded ? 0 : 1));

        if (isGrounded)
            agent.currentJumpCount = 0;
    }
}
