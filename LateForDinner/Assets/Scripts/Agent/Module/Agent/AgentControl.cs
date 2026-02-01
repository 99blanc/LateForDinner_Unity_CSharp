using System;
using System.Collections.Generic;
using Token.PRIORITY;
using UnityEngine;

public abstract class AgentControl<TView, TData, TKey> : MonoBehaviour, IAgentControl, IAgentModule<TView, TData, TKey> where TView : class, IViewProvider where TData : class, IData<TKey>
{
    private readonly Dictionary<Type, IAgentBehavior> behaviors = new();
    private readonly HashSet<Prop> props = new();
    public void HandleProp(Action<HashSet<Prop>> action) => action?.Invoke(props);
    public TData config { get; private set; }
    public StateMachine machine { get; private set; }
    public Rigidbody2D tBody { get; private set; }
    public CapsuleCollider2D tCollider { get; private set; }
    public Prop pProp { get; set; }
    public IActionView tView { get; private set; }
    public Vector2 moveInput { get; set; }
    public Vector2 lookAt { get; set; } = new();
    public bool isGrounded { get; set; }
    public bool isFalling { get; set; }
    public short currentJumpCount { get; set; }
    public virtual ModulePrority priority => ModulePrority.AGENT_CONTROL;

    public virtual void Setup(TData data, TView view)
    {
        config = data;
        machine = new();
        tBody = gameObject.GetOrAddComponentAssert<Rigidbody2D>();
        tCollider = gameObject.GetOrAddComponentAssert<CapsuleCollider2D>();
        tView = view as IActionView;
        Behaviors();
    }

    public Vector2 UpdateLookAt(Vector2 input) => input.y > 0 ? Vector2.up : new Vector2(input.x, input.y > 0 ? 0 : input.y).normalized;

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
