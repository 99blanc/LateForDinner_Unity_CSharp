using UnityEngine;

public class GravityBehavior<T> : IAgentBehavior<T> where T : class, IPhysicsData
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
        if (agent.isGrounded) 
            return;

        float isFalling = Mathf.Sign(Mathf.Min(0, agent.tBody.linearVelocity.y + Define.Physics.DEADZONE)) * -Define.Physics.FULL;
        float baseMultiplier = Define.Physics.FULL + (isFalling * (config.gvMul - Define.Physics.FULL - agent.tView.gvReduction.CurrentValue));
        float finalMultiplier = Mathf.Max(baseMultiplier, Define.Physics.FULL - Define.Physics.LIMIT);
        agent.tBody.AddForce(Vector2.down * -Physics2D.gravity.y * finalMultiplier, ForceMode2D.Force);
    }
}