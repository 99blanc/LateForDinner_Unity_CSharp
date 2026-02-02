using UnityEngine;

public class FallBehavior<T> : IAgentBehavior<T> where T : class, IFallData
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
        bool isClimbing = agent is IClimb && agent.hProp is IClimbProp;
        Vector2 bottom = new(agent.tCollider.bounds.center.x, agent.tCollider.bounds.min.y + Define.Physics.OFFSET);
        Vector2 castSize = new(agent.tCollider.bounds.size.x * Define.Physics.LIMIT, Define.Physics.OFFSET);
        RaycastHit2D hit = Physics2D.BoxCast(bottom, castSize, 0, Vector2.down, config.gcDistance, Define.Layer.GROUND_MASKS);
        bool detected = hit.collider != null && !hit.collider.isTrigger && agent.tBody.linearVelocity.y < Define.Physics.OFFSET;

        if (agent is IFall fall)
        {
            fall.isGrounded = detected && !isClimbing;
            fall.isFalling = !fall.isGrounded && agent.tBody.linearVelocity.y < Define.Physics.INTERVAL;
        }

        if (agent is IJump jAgent && agent is IFall fAgent)
            jAgent.currentJumpCount = fAgent.isGrounded ? (short)0 : jAgent.currentJumpCount;

        _ = (agent is IFall { isGrounded: false }) && Gravity();
    }

    private bool Gravity()
    {
        float isFalling = agent is IFall fall && fall.isFalling ? Define.Physics.FULL : 0;
        float baseMultiplier = Define.Physics.FULL + (isFalling * (config.gvMul - Define.Physics.FULL - agent.tView.gvReduction.CurrentValue));
        float finalMultiplier = Mathf.Max(baseMultiplier, Define.Physics.FULL - Define.Physics.LIMIT);
        agent.tBody.AddForce(Vector2.down * -Physics2D.gravity.y * finalMultiplier, ForceMode2D.Force);
        return true;
    }
}
