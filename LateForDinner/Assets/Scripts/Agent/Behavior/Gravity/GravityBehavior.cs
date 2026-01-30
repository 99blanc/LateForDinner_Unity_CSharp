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

    public void Execute(Vector2 input = default)
    {
        float isFalling = Mathf.Sign(Mathf.Min(0, agent.tBody.linearVelocity.y + Define.Physics.DEADZONE)) * -1f;
        float baseMultiplier = 1.0f + (isFalling * (config.gvMul - 1.0f - agent.tView.gvReduction.CurrentValue));
        float finalMultiplier = Mathf.Max(baseMultiplier, 1.0f - Define.Physics.GRAVITY_LIMIT);
        agent.tBody.AddForce(Vector2.down * -Physics2D.gravity.y * finalMultiplier, ForceMode2D.Force);
    }
}