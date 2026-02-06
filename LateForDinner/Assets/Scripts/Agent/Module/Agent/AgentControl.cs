using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using Token.PRIORITY;
using UnityEngine;

public abstract class AgentControl<TView, TData, TKey> : MonoBehaviour, IAgentControl, IAgentModule<TView, TData, TKey>, IPropHolder where TView : class, IViewProvider where TData : class, IData<TKey>
{
    private readonly Dictionary<Type, IAgentBehavior> behaviors = new();
    public abstract ModulePrority priority { get; }
    public ReactiveProperty<PropContext> props { get; private set; } = new(new());
    public TData config { get; private set; }
    public StateMachine sMachine { get; private set; }
    public Rigidbody2D tBody { get; private set; }
    public CapsuleCollider2D tCollider { get; private set; }
    public Prop active => props.Value.active;
    public IActionView tView { get; private set; }
    public Vector2 moveInput { get; set; }
    public bool isIdling { get; set; }
    public virtual bool isGrounded => this.IsGrounded();

    public virtual async UniTask Setup(TData data, TView view, StateMachine machine)
    {
        config = data;
        sMachine = machine;
        tBody = gameObject.GetOrAddComponentAssert<Rigidbody2D>();
        tCollider = gameObject.GetOrAddComponentAssert<CapsuleCollider2D>();
        tView = view as IActionView;
        Behaviors();
        await UniTask.CompletedTask;
    }

    public void ExecuteBehavior<T>(BehaviorContext context = default) where T : IAgentBehavior
    {
        if (behaviors.TryGetValue(typeof(T), out var behavior))
            behavior.Execute(context);
    }

    public T GetBehavior<T>() where T : IAgentBehavior => behaviors.TryGetValue(typeof(T), out var behavior) ? (T)behavior : default;

    protected void SetBehavior<T>(IAgentBehavior<T> behavior) where T : class, IData
    {
        var data = config as T;

        if (data is null)
            return;

        behavior.Setup(this, data);
        behaviors[behavior.GetType()] = behavior;
    }

    protected abstract void Behaviors();
}
