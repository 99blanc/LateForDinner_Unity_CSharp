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

    public void Prepare(BehaviorContext context) { }

    public void Execute(BehaviorContext context)
    {
        if (agent.isGrounded && agent.tBody.linearVelocity.y < Define.Physics.OFFSET && agent is IJump jump)
            jump.currentJumpCount.Value = 0;

        if (agent is IFall fall)
            fall.isFalling = !agent.isGrounded && agent.tBody.linearVelocity.y < Define.Physics.INTERVAL;

        Gravity();
    }

    public void Terminate(BehaviorContext context) { }

    private bool Gravity()
    {
        float isFalling = agent is IFall fall && fall.isFalling ? Define.Physics.FULL : 0;
        float baseMultiplier = Define.Physics.FULL + (isFalling * (config.gvMul - Define.Physics.FULL - agent.tView.gvReduction.CurrentValue));
        float finalMultiplier = Mathf.Max(baseMultiplier, Define.Physics.FULL - Define.Physics.LIMIT);
        agent.tBody.AddForce(Vector2.down * -Physics2D.gravity.y * finalMultiplier, ForceMode2D.Force);
        return true;
    }
}
