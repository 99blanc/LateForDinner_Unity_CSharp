using UnityEngine;

public class ClimbBehavior<T> : IAgentBehavior<T> where T : class, IClimbData
{
    private IAgentControl agent;
    private T config;
    private float xVelocity;

    public void Setup(IAgentControl control, T data)
    {
        agent = control;
        config = data;
    }

    public void Prepare()
    {
        agent.tBody.gravityScale = 0;
        agent.tBody.linearVelocity = Vector2.zero;
    }

    public void Execute(BehaviorContext context = default)
    {
        if (agent.active is not IClimbProp cProp)
            return;

        agent.tBody.linearVelocity = Vector2.zero;
        float nextX = Mathf.SmoothDamp(agent.tBody.position.x, cProp.centerX, ref xVelocity, Define.Physics.SNAP);
        float moveY = context.input.y * config.moveSpeed * config.decelObj * Time.fixedDeltaTime;
        float targetY = agent.tBody.position.y + moveY;
        float ladderTop = cProp.bounds.max.y;
        float footY = agent.tCollider.bounds.min.y;
        float pivotOffset = agent.tBody.position.y - footY;

        if (context.input.y > 0 && targetY > ladderTop + pivotOffset)
            targetY = ladderTop + pivotOffset + Define.Physics.DEADZONE;

        agent.tBody.MovePosition(new(nextX, targetY));
    }

    public void Terminate()
    {
        agent.tBody.linearVelocity = Vector2.zero;
        agent.tBody.gravityScale = Define.Physics.FULL;
    }

    public bool CanClimb(Vector2 input)
    {
        if (agent.active is not IClimbProp cProp || agent is not IClimb climb)
            return false;

        float ladderTop = cProp.bounds.max.y;
        float ladderBottom = cProp.bounds.min.y;
        float footY = agent.tCollider.bounds.min.y;
        bool isUp = input.y > 0;
        bool isDown = input.y < 0;
        bool isAtTop = footY >= ladderTop - Define.Physics.OFFSET;
        bool tryingToGoUpAtTop = isUp && isAtTop && agent.isGrounded;
        bool hasMoreLadderBelow = footY > ladderBottom + Define.Physics.OFFSET;
        bool tryingToExitFromGround = agent.isGrounded && !climb.isClimbing && ((isDown && !hasMoreLadderBelow) || input.x != 0);
        bool isAtBottom = footY < ladderBottom + Define.Physics.OFFSET;
        bool shouldReleaseAtBottom = climb.isClimbing && agent.isGrounded && isAtBottom && (isDown || input.x != 0);

        if (tryingToGoUpAtTop || shouldReleaseAtBottom || tryingToExitFromGround)
            return false;

        bool canUp = isUp && footY < ladderTop;
        bool canDown = isDown && footY < ladderTop + Define.Physics.OFFSET;
        return (climb.isClimbing || canUp || canDown);
    }
}