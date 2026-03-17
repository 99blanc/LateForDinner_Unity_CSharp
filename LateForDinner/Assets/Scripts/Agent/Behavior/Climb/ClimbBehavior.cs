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
        float targetX = agent.isGrounded ? agent.tBody.position.x : cProp.centerX;
        float nextX = Mathf.SmoothDamp(agent.tBody.position.x, targetX, ref xVelocity, Define.Physics.SNAP);
        float moveY = context.input.y * agent.tView.moveSpeed.CurrentValue * config.decelObj * Time.fixedDeltaTime;
        float nextY = agent.tBody.position.y + moveY;
        agent.tBody.MovePosition(new(nextX, nextY));
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

        if (climb.isClimbing)
        {
            bool outOfBounds = footY > ladderTop + Define.Physics.HALF || footY < ladderBottom - Define.Physics.HALF;
            return !outOfBounds;
        }

        bool withinVerticalRange = footY <= ladderTop && footY >= ladderBottom;
        return withinVerticalRange && (isUp || isDown);
    }
}