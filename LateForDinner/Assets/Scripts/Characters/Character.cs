using R3;
using UnityEngine;
using UnityHFSM;

public abstract class Character : MonoBehaviour, IPoolable
{
    public AttributeRegistry Attributes { get; protected set; } = new();
    public Animator Animator { get; private set; }
    public abstract CharacterID CharacterID { get; }
    protected StateMachine<CharacterState> StateMachine;

    public virtual void Init()
    {
        Animator = this.GetComponentAssert<Animator>();
        StateMachine = new StateMachine<CharacterState>();
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
}
