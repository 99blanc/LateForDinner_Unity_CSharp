using R3;
using UnityEngine;
using UnityHFSM;

public abstract class Character : MonoBehaviour, IPoolable
{
    public AttributeRegistry Attributes { get; protected set; } = new();
    public SpriteRenderer Renderer { get; private set; }
    public Animator Animator { get; private set; }
    protected abstract CharacterAnimator CharacterAnimator { get; }
    protected abstract CharacterID CharacterID { get; }
    protected StateMachine<CharacterStateType> StateMachine;

    public virtual void Init()
    {
        InitStateMachine();
        CacheComponents();
    }

    protected virtual void InitStateMachine()
    {
        StateMachine = new StateMachine<CharacterStateType>();
        RegisterStates(StateMachine);
        RegisterTransitions(StateMachine);
        StateMachine.SetStartState(CharacterStateType.Idle);
        StateMachine.Init();
        Observable.EveryUpdate()
        .Subscribe(_ => StateMachine.OnLogic())
        .AddToPool(this);
    }

    public virtual void Get()
    {

    }

    public virtual void Release()
    {

    }

    protected virtual void RegisterStates(StateMachine<CharacterStateType> fsm) { }
    protected virtual void RegisterTransitions(StateMachine<CharacterStateType> fsm) { }

    protected virtual void CacheComponents()
    {
        Renderer = this.FindChildAssert<SpriteRenderer>(recursive: true);
        Animator = this.FindChildAssert<Animator>(recursive: true);
        CharacterAnimator?.SetAnimator(Animator);
    }
}
