using ObservableCollections;
using R3;
using UnityEngine;
using Token.PRIORITY;

public abstract class Prop : MonoBehaviour, IProp
{
    protected readonly ObservableHashSet<IAgentControl> occupants = new();
    public abstract PropPriority priority { get; }
    public Collider2D sensor { get; set; }
    public Transform rTransform => transform;
    public GameObject rGameObject => gameObject;

    protected virtual void Awake()
    {
        sensor = gameObject.GetOrAddComponentAssert<Collider2D>();
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

    public virtual void SetActive(bool active)
    {
        if (sensor is not null) 
            sensor.enabled = active;
    }
}
