using UnityEngine;

public class ThrowBehavior<T> : IAgentBehavior<T> where T : class, IThrowData
{
    private IAgentControl agent;
    private T config;

    public void Setup(IAgentControl control, T data)
    {
        agent = control;
        config = data;
    }

    public void Prepare() { }

    public void Execute(BehaviorContext context = default)
    {
        var throwable = agent.itemSocket.GetComponentInChildrenAssert<IThrowProp>();

        if (throwable is null || !throwable.CanThrow(agent)) 
            return;

        if (throwable.rGameObject.TryGetComponent<Rigidbody2D>(out var body) is false) 
            return;

        throwable.rTransform.SetParent(null);
        throwable.SetActive(true);
        body.linearVelocity = Vector2.zero;
        Vector2 input = context.input;
        Vector2 throwDir = agent.moveInput.x >= 0 ? Vector2.right : Vector2.left;
        float power = 0;

        if (input.x != 0)
        {
            throwDir = new Vector2(input.x, input.y > 0 ? 1f : 0).normalized;
            power = config.throwPower;
        }

        if (input.x == 0 && input.y > 0)
        {
            throwDir = input.y > 0 ? Vector2.up : Vector2.down;
            power = config.throwPower;
        }

        body.AddForce(throwDir * power, ForceMode2D.Impulse);
        body.AddTorque(power > 0 ? config.throwTorque : config.throwTorque * Define.Physics.TICK, ForceMode2D.Impulse);
        throwable.OnDetach(agent);
    }

    public void Terminate() { }
}
