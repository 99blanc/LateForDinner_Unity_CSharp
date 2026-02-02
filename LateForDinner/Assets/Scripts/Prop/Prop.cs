using R3;
using System.Collections.Generic;
using Token.PRIORITY;
using UnityEngine;

public abstract class Prop : MonoBehaviour, IProp
{
    private readonly CompositeDisposable disposables = new();
    private readonly ReactiveProperty<IAgentControl> occupant = new(null);
    private readonly HashSet<IAgentControl> agents = new();
    public BoxCollider2D sensor { get; set; }
    public virtual PropPriority priority => default;

    protected virtual void Awake()
    {
        sensor = gameObject.GetOrAddComponentAssert<BoxCollider2D>();
        occupant.Select(agent => agent is not null ? Observable.EveryUpdate(UnityFrameProvider.FixedUpdate) : Observable.Never<Unit>()).Switch().Subscribe(_ => OnTick(occupant.Value)).AddTo(disposables);
    }

    public virtual void OnTick(IAgentControl agent) { }

    public virtual void OnInteract(IAgentControl agent) => occupant.Value = agent;

    public virtual void OnDetach(IAgentControl agent) => occupant.Value = null;

    protected void HandleEnter(GameObject gameObject)
    {
        if (gameObject.TryGetComponent<IAgentControl>(out var agent) && agents.Add(agent) && occupant.Value == null)
        {
            agent.InProp(this);
            OnInteract(agent);
        }
    }

    protected void HandleExit(GameObject gameObject)
    {
        if (gameObject.TryGetComponent<IAgentControl>(out var agent) && agents.Remove(agent) && occupant.Value == agent)
        {
            agent.OutProp(this);
            OnDetach(agent);
        }
    }

    protected virtual void OnDestroy() => disposables.Dispose();
}
