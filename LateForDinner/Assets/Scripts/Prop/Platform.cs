using NUnit.Framework.Internal;
using Token.PRIORITY;
using UnityEngine;

public class Platform : Prop
{
    public override PropPriority priority => PropPriority.PLATFORM;

    private BoxCollider2D physics;

    protected override void Awake()
    {
        base.Awake();
        physics = gameObject.AddComponent<BoxCollider2D>();
        physics.size = cCollider.size;
        physics.offset = cCollider.offset;
        physics.isTrigger = false;
        physics.enabled = false;
    }

    public override void OnTick(IAgentControl agent)
    {
        Prop prop = agent.GetProp();

        if (prop != null && physics.enabled && (int)prop.priority < (int)this.priority)
        {
            physics.enabled = false;
            return;
        }

        float footY = agent.tCollider.bounds.min.y + Define.Physics.PLATFORM_OFFSET;
        float topY = cCollider.bounds.max.y;
        bool isAbove = footY >= topY;
        bool isNotJumpingUp = agent.tBody.linearVelocity.y <= 0;
        bool wantsToDrop = agent.moveInput.y < -Define.Physics.DEADZONE;
        bool shouldEnable = isAbove && isNotJumpingUp && !wantsToDrop;

        if (physics.enabled != shouldEnable)
            physics.enabled = shouldEnable;
    }

    public override void OnDetach(IAgentControl agent)
    {
        base.OnDetach(agent);

        if (physics is not null) 
            physics.enabled = false;
    }
}
