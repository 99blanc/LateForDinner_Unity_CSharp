using Token.PRIORITY;

public abstract class ThrowProp : PickupProp, IThrowProp
{
    public virtual bool CanThrow(IAgentControl agent) => transform.parent == agent.itemSocket;
}
