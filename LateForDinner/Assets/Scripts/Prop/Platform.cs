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
        bool isClimbing = agent is PlayerControl player && player.machine.curState == player.ladderState;
        bool isDownInput = agent.moveInput.y < -Define.Physics.DEADZONE;
        bool hasLadder = agent.pProp is ILadderProp;
        bool isDownThrough = isDownInput && hasLadder;
        float verticalVelocity = agent.tBody.linearVelocity.y;
        float velocityBuffer = verticalVelocity < 0 ? Mathf.Abs(verticalVelocity) * Time.fixedDeltaTime : 0;
        bool isAbove = (footY + velocityBuffer) >= topY;
        bool finalEnabled = isAbove && !isDownThrough && !isClimbing;

        if (physics.enabled != finalEnabled)
            physics.enabled = finalEnabled;

        bool canClimb = isDownThrough && agent is ILadderAgent;

        if (canClimb)
            ((ILadderAgent)agent).EnslaveToLadder();
    }

    public override void OnDetach(IAgentControl agent)
    {
        base.OnDetach(agent);

        if (physics is not null) 
            physics.enabled = false;
    }
}
