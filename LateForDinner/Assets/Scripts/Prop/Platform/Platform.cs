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
        physics.isTrigger = false;
        physics.enabled = false;
    }

    public abstract override void OnTick(IAgentControl agent);

    public override void OnDetach(IAgentControl agent)
    {
        base.OnDetach(agent);

        if (physics != null)
            physics.enabled = false;
    }

    protected bool Evaluate(IAgentControl agent, bool dropable)
    {
        float topY = sensor.bounds.max.y;
        float footY = agent.tCollider.bounds.min.y + Define.Physics.OFFSET;
        bool isDown = agent.moveInput.y < -Define.Physics.DEADZONE;
        float vVel = agent.tBody.linearVelocity.y;
        bool isAbove = footY >= topY - (vVel < 0 ? Mathf.Abs(vVel) * Time.fixedDeltaTime : 0);
        bool isClimbing = agent.hProp is IClimbProp && agent is IClimb { isClimbing: true };
        return !isClimbing && isAbove && (!dropable || !isDown);
    }

    protected void Toggle(bool state)
    {
        if (physics.enabled != state)
            physics.enabled = state;
    }
}