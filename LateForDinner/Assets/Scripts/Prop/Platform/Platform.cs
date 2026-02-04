using UnityEngine;

public abstract class Platform : TriggerProp, IPlatformProp
{
    protected BoxCollider2D physics { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        physics = gameObject.AddComponent<BoxCollider2D>();
        physics.size = sensor.size;
        physics.offset = sensor.offset;
    }

    public abstract override void OnTick(IAgentControl agent);

    public override void OnDetach(IAgentControl agent)
    {
        base.OnDetach(agent);
        Physics2D.IgnoreCollision(agent.tCollider, physics, false);
    }

    protected bool Evaluate(IAgentControl agent, bool dropable)
    {
        float platformTop = sensor.bounds.max.y;
        float footY = agent.tCollider.bounds.min.y;
        bool isAbove = footY >= platformTop - Define.Physics.OFFSET;
        bool isClimbing = agent is IClimb climb && climb.isClimbing;
        bool isFalling = agent is IFall fall && fall.isFalling;
        bool isDownInput = agent.moveInput.y < 0;
        bool isTumbling = agent is ITumble tumbler && (tumbler.isTumbling || (tumbler.isSneaking && tumbler.isJumping));

        if (isClimbing || (agent.active is IClimbProp && isDownInput))
            return false;

        if (dropable && isTumbling)
        {
            SetIgnore(agent, true);
            return false;
        }

        if (dropable && isFalling && isDownInput)
            return false;

        return isAbove;
    }

    protected void SetIgnore(IAgentControl agent, bool ignore) => Physics2D.IgnoreCollision(agent.tCollider, physics, ignore);
}
