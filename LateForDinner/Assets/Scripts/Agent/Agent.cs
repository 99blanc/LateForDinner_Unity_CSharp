using UnityEngine;
using ZLinq;

public abstract class Agent<TComponent, TView, TData, TKey> : MonoBehaviour where TComponent : Component where TView : class, IViewProvider where TData : IData<TKey>
{
    public TData aData { get; private set; }
    public IAgentModule<TView, TData, TKey>[] modules { get; protected set; }
    public StatModel registry { get; protected set; } = new();
    public StateMachine machine { get; private set; } = new();
    public TView view => registry as TView;

    public virtual void Init(TData data)
    {
        Components();
        ApplyRegistry(data);
        SetupModule(data);
    }

    protected abstract void Components();

    protected abstract void ApplyRegistry(TData data);

    private void SetupModule(TData data)
    {
        aData = data;
        var founds = gameObject.GetComponentsAssert<IAgentModule<TView, TData, TKey>>();
        modules = founds.AsValueEnumerable().OrderBy(m => (int)m.priority).ToArray();

        foreach (var module in modules)
            module.Setup(data, view, machine);
    }
}
