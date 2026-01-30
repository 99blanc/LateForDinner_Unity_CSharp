using System;
using System.Collections.Generic;
using UnityEngine;
using Token.PRIORITY;

public abstract class AgentControl<TView, TData, TKey> : MonoBehaviour, IAgentControl, IAgentModule<TView, TData, TKey> where TView : class, IViewProvider where TData : class, IData<TKey>
{
    private readonly Dictionary<Type, IAgentBehavior> behaviors = new();
    protected TData config;
    public StateMachine machine { get; private set; }
    public Rigidbody2D tBody { get; private set; }
    public CapsuleCollider2D tCollider { get; private set; }
    public Collider2D actCollider { get; set; }
    public IActionView tView { get; private set; }
    public Vector2 moveInput { get; protected set; }
    public Vector2 lookAt { get; protected set; } = new();
    public bool isNearGround { get; set; }
    public short currentJumpCount { get; set; }
    public ModulePrority priority => ModulePrority.AGENT_CONTROL;

    public virtual void Setup(TData data, TView view)
    {
        config = data;
        machine = new();
        tBody = gameObject.GetOrAddComponentAssert<Rigidbody2D>();
        tCollider = gameObject.GetOrAddComponentAssert<CapsuleCollider2D>();
        tView = view as IActionView;
        Behaviors();
    }

    public void ExecuteBehavior<T>(Vector2 input = default) where T : IAgentBehavior
    {
        if (behaviors.TryGetValue(typeof(T), out var behavior))
            behavior.Execute(input);
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
