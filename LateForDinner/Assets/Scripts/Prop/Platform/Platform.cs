using UnityEngine;

public abstract class Platform : PhysicsProp, IPlatformProp
{
    public abstract bool dropable { get; }

    protected bool Evaluate(IAgentControl agent)
    {
        float platformTop = physics.bounds.max.y;
        float footY = agent.tCollider.bounds.min.y;
        bool isAbove = footY > platformTop - Define.Physics.SNAP;

        if (agent is IClimb { isClimbing: true })
            return false;

        if (dropable && agent is ITumble { isTumbling: true })
            return false;

        return isAbove;
    }

    protected void SetIgnore(IAgentControl agent, bool ignore) => Physics2D.IgnoreCollision(agent.tCollider, physics, ignore);
}
