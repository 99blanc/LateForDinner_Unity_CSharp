using R3;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using Token.PRIORITY;

public abstract class AgentAnimator<TView, TData, TKey> : MonoBehaviour, IAgentAnimator, IAgentModule<TView, TData, TKey> where TView : class, IViewProvider where TData : class, IData<TKey>
{
    private readonly Dictionary<Type, int> hashes = new();
    public abstract ModulePrority priority { get; }
    public StateMachine sMachine { get; private set; }
    public Rigidbody2D tBody { get; private set; }
    public CapsuleCollider2D tCollider { get; private set; }
    public Vector2 lookAt { get; protected set; }
    public IAgentControl control { get; protected set; }
    public IAgentView aView { get; protected set; }
    protected Animator animator;
    public abstract string cPath { get; }

    public virtual async UniTask Setup(TData data, TView view, StateMachine machine)
    {
        sMachine = machine;
        tBody = gameObject.GetOrAddComponentAssert<Rigidbody2D>();
        tCollider = gameObject.GetOrAddComponentAssert<CapsuleCollider2D>();
        animator = gameObject.GetOrAddComponentAssert<Animator>();
        animator.runtimeAnimatorController = await Managers.Resource.LoadAnimator(cPath);
        animator.updateMode = AnimatorUpdateMode.Fixed;
        control = gameObject.GetComponentAssert<IAgentControl>();
        aView = view as IAgentView;
        sMachine.OnStateChanged.Where(state => state is not null).Subscribe(state => PlayStateAnimation(state.GetType())).AddTo(this);
        Observable.EveryUpdate(UnityFrameProvider.FixedUpdate).Subscribe(_ => Parameters()).AddTo(this);
        States();
    }

    protected void RegisterState<TSpecific>() where TSpecific : State
    {
        Type baseType = typeof(TSpecific).BaseType;

        if (baseType == null) 
            return;

        ReadOnlySpan<char> nameSpan = baseType.Name.AsSpan();
        int backtickIndex = nameSpan.IndexOf('`');

        if (backtickIndex != -1)
            nameSpan = nameSpan.Slice(0, backtickIndex);

        int nameHash = Animator.StringToHash(nameSpan.ToString());

        if (animator.HasState(0, nameHash))
            hashes[typeof(TSpecific)] = nameHash;
    }

    protected virtual void PlayStateAnimation(Type stateType)
    {
        if (hashes.TryGetValue(stateType, out var hash))
            Play(hash);
    }

    protected abstract void States();

    protected abstract void Parameters();

    protected void Flip(float xDir)
    {
        if (Mathf.Abs(xDir) < Define.Physics.DEADZONE) 
            return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (xDir > 0 ? 1 : -1);
        transform.localScale = scale;
    }

    public void Play(int hash, float transition = Define.Physics.BUFFER) => animator.CrossFade(hash, transition, 0);

    public void SetParam(int hash, float value) => animator.SetFloat(hash, value);

    public void SetParam(int hash, bool value) => animator.SetBool(hash, value);

    public void SetParam(int hash, int value) => animator.SetInteger(hash, value);
}
