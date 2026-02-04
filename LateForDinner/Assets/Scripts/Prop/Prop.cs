using ObservableCollections;
using R3;
using UnityEngine;
using Token.PRIORITY;

public abstract class Prop : MonoBehaviour, IProp
{
    protected readonly ObservableHashSet<IAgentControl> occupants = new();
    public BoxCollider2D sensor { get; set; }
    public abstract PropPriority priority { get; }

    protected virtual void Awake()
    {
        sensor = gameObject.GetOrAddComponentAssert<BoxCollider2D>();
        Observable.EveryUpdate(UnityFrameProvider.FixedUpdate).Where(_ => occupants.Count > 0).Subscribe(_ =>
        {
            foreach (var agent in occupants)
                OnTick(agent);
        })
        .AddTo(this);
    }

    public virtual void OnTick(IAgentControl agent) { }

    public virtual void OnInteract(IAgentControl agent) => occupants.Add(agent);

    public virtual void OnDetach(IAgentControl agent) => occupants.Remove(agent);

    protected void HandleEnter(GameObject gameObject)
    {
        if (gameObject.TryGetComponent<IAgentControl>(out var agent))
        {
            agent.Occupy(this);
            OnInteract(agent);
        }
    }

    protected void HandleExit(GameObject gameObject)
    {
        if (gameObject.TryGetComponent<IAgentControl>(out var agent))
        {
            agent.Release(this);
            OnDetach(agent);
        }
    }
}
