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

    public void Prepare(BehaviorContext context)
    {
        agent.tBody.gravityScale = 0;
        agent.tBody.linearVelocity = Vector2.zero;
    }

    public void Execute(BehaviorContext context)
    {
        if (agent.active is not IClimbProp cProp)
            return;

        Vector2 input = context.input;

        if (!Climb(cProp, input))
        {
            agent.tBody.linearVelocity = Vector2.zero;
            return;
        }

        float nextX = Mathf.SmoothDamp(agent.tBody.position.x, cProp.centerX, ref xVelocity, Define.Physics.SNAP);
        float moveY = input.y * config.moveSpeed * config.decelObj * Time.fixedDeltaTime;
        agent.tBody.MovePosition(new(nextX, agent.tBody.position.y + moveY));
    }

    public void Terminate(BehaviorContext context)
    {
        agent.tBody.linearVelocity = Vector2.zero;
        agent.tBody.gravityScale = Define.Physics.FULL;
    }

    public bool CanClimb(Vector2 input)
    {
        if (agent.active is not IClimbProp ladder || agent is not IClimb climb)
            return false;

        bool hasVerticalInput = input.y != 0;
        bool isDown = input.y < -Define.Physics.DEADZONE;
        float footY = agent.tCollider.bounds.min.y;
        float ladderTop = ladder.bounds.max.y;
        bool isAtTop = footY > ladderTop - Define.Physics.OFFSET;
        bool tryingToEnterFromTop = isDown && footY <= ladderTop + Define.Physics.OFFSET;
        bool tryingToGoUpAtTop = input.y > 0 && isAtTop;
        bool isGrounded = agent.isGrounded;
        bool tryingToExitFromGround = isGrounded && (input.y < -Define.Physics.DEADZONE || input.x != 0);

        if (tryingToEnterFromTop) 
            return true;

        return (climb.isClimbing || hasVerticalInput) && !tryingToGoUpAtTop && !tryingToExitFromGround;
    }

    private bool Climb(IClimbProp cProp, Vector2 input)
    {
        if (input.y == 0) 
            return true;

        float ladderTop = cProp.bounds.max.y;
        float ladderBottom = cProp.bounds.min.y;
        float footY = agent.tCollider.bounds.min.y;
        float headY = agent.tCollider.bounds.max.y;
        float finalY = (footY + agent.tCollider.bounds.center.y) * Define.Physics.HALF;
        bool canUp = input.y > 0 && footY <= ladderTop + Define.Physics.OFFSET && headY > ladderBottom;
        bool canDown = input.y < 0 && footY < ladderTop;
        return canUp || canDown;
    }
}
