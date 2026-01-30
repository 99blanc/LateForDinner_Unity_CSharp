using UnityEngine;

public abstract class Prop : MonoBehaviour, IInteractProp
{
    private Collider2D cCollider;

    protected virtual void Awake() => cCollider = gameObject.GetComponentAssert<Collider2D>();

    public abstract void OnInteract(IAgentControl agent);

    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.TryGetComponent(out IAgentControl agent))
            agent.actCollider = cCollider;
    }

    protected virtual void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.TryGetComponent(out IAgentControl agent))
            OnInteract(agent);
    }

    protected virtual void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.TryGetComponent(out IAgentControl agent) && agent.actCollider == cCollider)
            agent.actCollider = null;
    }
}
