using R3;
using Token.PRIORITY;
using UnityEngine;

public abstract class Prop : MonoBehaviour, IProp
{
    private readonly ReactiveProperty<IAgentControl> occupant = new(null);
    public BoxCollider2D sensor { get; set; }
    public virtual PropPriority priority => default;

    protected virtual void Awake()
    {
        sensor = gameObject.GetOrAddComponentAssert<BoxCollider2D>();
        occupant.Select(agent => agent is not null ? Observable.EveryUpdate(UnityFrameProvider.FixedUpdate) : Observable.Never<Unit>()).Switch().Subscribe(o => OnTick(occupant.Value)).AddTo(this);
    }

    public virtual void OnTick(IAgentControl agent) { }

    public virtual void OnInteract(IAgentControl agent) => occupant.Value = agent;

    public virtual void OnDetach(IAgentControl agent) => occupant.Value = null;

    protected void HandleEnter(GameObject gameObject)
    {
        if (gameObject.TryGetComponent<IAgentControl>(out var agent) && occupant.Value == null)
        {
            agent.hProp.InProp(this);
            OnInteract(agent);
        }
    }

    protected void HandleExit(GameObject gameObject)
    {
        if (gameObject.TryGetComponent<IAgentControl>(out var agent) && occupant.Value == agent)
        {
            agent.hProp.OutProp(this);
            OnDetach(agent);
        }
    }
}
