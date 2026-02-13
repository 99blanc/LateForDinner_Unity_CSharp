public abstract class PickupProp : PhysicsProp, IPickupProp
{
    public virtual bool CanPickup(IAgentControl agent) => gameObject.activeSelf && transform.parent is null;
}
