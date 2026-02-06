using UnityEngine;

public abstract class Platform : TriggerProp, IPlatformProp
{
    public abstract bool dropable { get; }
    protected BoxCollider2D physics { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        physics = gameObject.AddComponent<BoxCollider2D>();
        physics.size = sensor.size;
        physics.offset = sensor.offset;
        float margin = Define.Physics.OFFSET * Define.Physics.DOUBLE;
        sensor.size = new(physics.size.x, physics.size.y + margin);
    }

    protected bool Evaluate(IAgentControl agent)
    {
        float platformTop = physics.bounds.max.y;
        float footY = agent.tCollider.bounds.min.y;
        bool isAbove = footY > platformTop;

        if (agent is IClimb { isClimbing: true })
            return false;

        if (dropable && agent is ITumble { isTumbling: true })
            return false;

        return isAbove;
    }

    protected void SetIgnore(IAgentControl agent, bool ignore) => Physics2D.IgnoreCollision(agent.tCollider, physics, ignore);
}
