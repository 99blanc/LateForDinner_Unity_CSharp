using Token.PRIORITY;
using UnityEngine;

public class Platform : Prop, IPlatformProp
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

    public override void OnTick(IAgentControl agent)
    {
        float topY = cCollider.bounds.max.y;
        float footY = agent.tCollider.bounds.min.y + Define.Physics.OFFSET;
        bool isClimbing = agent is PlayerControl p && p.machine.curState == p.ladderState;
        bool isDownInput = agent.moveInput.y < -Define.Physics.DEADZONE;
        bool hasLadder = agent.pProp is ILadderProp;
        float vVel = agent.tBody.linearVelocity.y;
        float velocityBuffer = vVel < 0 ? Mathf.Abs(vVel) * Time.fixedDeltaTime : 0;
        bool isAbove = (footY + velocityBuffer) >= topY;
        bool finalEnabled = isAbove && !isDownInput && !isClimbing;

        if (physics.enabled != finalEnabled)
            physics.enabled = finalEnabled;

        if (isDownInput && hasLadder && agent is ILadderAgent ladderAgent)
            ladderAgent.EnslaveToLadder();
    }

    public override void OnDetach(IAgentControl agent)
    {
        base.OnDetach(agent);

        if (physics is not null) 
            physics.enabled = false;
    }
}
