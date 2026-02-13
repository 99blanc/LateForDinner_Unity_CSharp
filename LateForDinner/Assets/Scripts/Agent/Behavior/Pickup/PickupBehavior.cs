using UnityEngine;

public class PickupBehavior<T> : IAgentBehavior<T> where T : class, IPickupData
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
        if (agent.active is not IPickupProp pickup || !pickup.CanPickup(agent))
            return;

        pickup.SetActive(false);
        pickup.rTransform.SetParent(agent.itemSocket);
        pickup.rTransform.localPosition = Vector3.zero;
        pickup.rTransform.localRotation = Quaternion.identity;
        pickup.OnInteract(agent);
    }

    public void Terminate() { }
}
