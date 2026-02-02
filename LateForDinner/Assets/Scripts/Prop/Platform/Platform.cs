using UnityEngine;
using Token.PRIORITY;

public abstract class Platform : Prop, IPlatformProp
{
    public override PropPriority priority => PropPriority.PLATFORM;
    public BoxCollider2D physics { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        physics = gameObject.AddComponent<BoxCollider2D>();
        physics.size = cCollider.size;
        physics.offset = cCollider.offset;
        physics.isTrigger = false;
        physics.enabled = false;
    }

    protected bool Evaluate(IAgentControl agent, bool dropable)
    {
        float topY = cCollider.bounds.max.y;
        float footY = agent.tCollider.bounds.min.y + Define.Physics.OFFSET;
        bool isClimbing = agent is PlayerControl p && p.machine.curState == p.ladderState;
        bool isDown = agent.moveInput.y < -Define.Physics.DEADZONE;
        float vVel = agent.tBody.linearVelocity.y;
        float buffer = vVel < 0 ? Mathf.Abs(vVel) * Time.fixedDeltaTime : 0;
        bool isAbove = (footY + buffer) >= topY;

        if (isDown && agent.pProp is ILadderProp && agent is ILadderAgent ladderAgent)
            ladderAgent.UseLadder();

        return isAbove && !isClimbing && (!dropable || !isDown);
    }

    protected void Toggle(bool state)
    {
        if (physics.enabled != state)
            physics.enabled = state;
    }

    public override void OnDetach(IAgentControl agent)
    {
        base.OnDetach(agent);

        if (physics != null) 
            physics.enabled = false;
    }

    public abstract override void OnTick(IAgentControl agent);
}