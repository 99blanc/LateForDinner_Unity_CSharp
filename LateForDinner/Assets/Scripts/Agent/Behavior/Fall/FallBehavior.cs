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

    public void Prepare() { }

    public void Execute(BehaviorContext context = default)
    {
        if (agent is IDash dash && dash.isDashing) 
            return;

        bool isGrounded = agent.isGrounded;

        if (isGrounded && agent is IJump jump)
            jump.currentJumpCount.Value = 0;

        if (!isGrounded && agent is IFall fall)
            fall.isFalling = agent.tBody.linearVelocity.y < Define.Physics.OFFSET;

        Gravity();
    }

    public void Terminate() { }

    private void Gravity()
    {
        float isFalling = agent is IFall fall && fall.isFalling ? Define.Physics.FULL : 0;
        float baseMultiplier = Define.Physics.FULL + (isFalling * (config.gvMul - Define.Physics.FULL - agent.tView.gvReduction.CurrentValue));
        float finalMultiplier = Mathf.Max(baseMultiplier, Define.Physics.FULL - Define.Physics.LIMIT);
        agent.tBody.AddForce(Vector2.down * -Physics2D.gravity.y * finalMultiplier, ForceMode2D.Force);
    }
}
