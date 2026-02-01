using R3;
using System.Collections.Generic;
using Token.PRIORITY;
using UnityEngine;

public abstract class Prop : MonoBehaviour, IInteractProp
{
    private readonly CompositeDisposable disposables = new();
    private readonly ReactiveProperty<IAgentControl> occupant = new(null);
    private readonly HashSet<IAgentControl> agents = new();
    public BoxCollider2D cCollider { get; private set; }
    public virtual PropPriority priority => default;

    protected virtual void Awake()
    {
        cCollider = gameObject.GetComponentAssert<BoxCollider2D>();
        occupant.Select(agent => agent is not null ? Observable.EveryUpdate(UnityFrameProvider.FixedUpdate) : Observable.Never<Unit>()).Switch().Subscribe(_ => OnTick(occupant.Value)).AddTo(disposables);
    }

    public virtual void OnTick(IAgentControl agent) { }

    public virtual void OnInteract(IAgentControl agent) => occupant.Value = agent;

    public virtual void OnDetach(IAgentControl agent) => occupant.Value = null;

    private void OnTriggerEnter2D(Collider2D collider) => HandleEnter(collider.gameObject);

    private void OnTriggerStay2D(Collider2D collider) => HandleStay();

    private void OnTriggerExit2D(Collider2D collider) => HandleExit(collider.gameObject);

    private void HandleEnter(GameObject gameObject)
    {
        if (gameObject.TryGetComponent<IAgentControl>(out var agent) && agents.Add(agent) && occupant.Value == null)
        {
            agent.InProp(this);
            OnInteract(agent);
        }
    }

    private void HandleStay()
    {
        if (occupant.Value == null && agents.Count > 0)
        {
            foreach (var agent in agents)
            {
                agent.InProp(this);
                OnInteract(agent);
                break;
            }
        }
    }

    private void HandleExit(GameObject gameObject)
    {
        if (gameObject.TryGetComponent<IAgentControl>(out var agent) && agents.Remove(agent) && occupant.Value == agent)
        {
            agent.OutProp(this);
            OnDetach(agent);
        }
    }

    protected virtual void OnDestroy() => disposables.Dispose();
}
