using UnityEngine;

public class MoveBehavior<T> : IAgentBehavior<T> where T : class, IMoveData
{
    private IAgentControl agent;
    private T config;

    public void Setup(IAgentControl control, T data)
    {
        agent = control;
        config = data;
    }

    public void Execute(Vector2 input)
    {
        float moveSpeed = agent.tView.moveSpeed.CurrentValue;
        float targetSpeed = input.x * moveSpeed;
        float isInputting = (Mathf.Abs(input.x) > Define.Physics.DEADZONE) ? 1f : 0;
        float accelRate = Mathf.Lerp(config.deceleration, config.acceleration, isInputting);
        float isTurning = Mathf.Clamp01(Mathf.Sign(targetSpeed) * Mathf.Sign(agent.tBody.linearVelocity.x) * -1 + 1);
        accelRate *= Mathf.Lerp(1f, config.turnVel, isTurning * isInputting);
        float accelAmount = accelRate * moveSpeed * Time.fixedDeltaTime;
        float newX = Mathf.MoveTowards(agent.tBody.linearVelocity.x, targetSpeed, accelAmount);
        float newY = agent.tBody.linearVelocity.y;

        if (agent.tBody.gravityScale == 0 && Mathf.Abs(input.y) <= Define.Physics.DEADZONE)
            newY = Mathf.MoveTowards(newY, 0, config.deceleration * moveSpeed * Time.fixedDeltaTime);

        agent.tBody.linearVelocity = new Vector2(newX, newY);
    }
}
