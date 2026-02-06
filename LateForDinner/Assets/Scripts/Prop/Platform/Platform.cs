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
        bool isAbove = footY > platformTop - Define.Physics.OFFSET;
        bool isDownInput = agent.moveInput.y < 0;

        if (dropable && agent is ITumble { isTumbling: true })
            return false;

        if (agent is IClimb { isClimbing: true } || (agent.active is IClimbProp && isDownInput))
            return false;

        if (dropable && isDownInput && agent is IFall { isFalling: true })
            return false;

        return isAbove;
    }

    protected void SetIgnore(IAgentControl agent, bool ignore) => Physics2D.IgnoreCollision(agent.tCollider, physics, ignore);
}
