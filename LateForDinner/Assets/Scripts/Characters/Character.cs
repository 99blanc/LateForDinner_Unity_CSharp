using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityHFSM;

public abstract class Character : MonoBehaviour, IPoolablePrefab
{
    public AttributeRegistry Attributes { get; protected set; } = new AttributeRegistry();
    public SpriteRenderer Renderer { get; private set; }
    public Animator Animator { get; private set; }
    public Collider2D Collider { get; private set; }
    public StateMachine<CharacterStateType> StateMachine;
    public abstract CharacterAnimator CharacterAnimator { get; }
    protected abstract CharacterID CharacterID { get; }

    public virtual async UniTask InitAsync()
    {
        CacheComponents();
        CharacterAnimator.SetAnimator(Animator);
        await GetAnimatorControllerAsync();
        InitStateMachine();
    }

    private async UniTask GetAnimatorControllerAsync()
    {
        string overrideControllerPath = CharacterID.GetAnimatorOverrideControllerPath();
        AnimatorOverrideController overrideController = await Managers.Resource.LoadAnimatorOverrideControllerAsync(overrideControllerPath);

        if (overrideController != null && CharacterAnimator != null)
            CharacterAnimator.SetOverrideController(overrideController);
    }

    protected virtual void InitStateMachine()
    {
        StateMachine = new StateMachine<CharacterStateType>();
        RegisterStates(StateMachine);
        RegisterTransitions(StateMachine);
        StateMachine.SetStartState(CharacterStateType.Idle);
        StateMachine.Init();
        Observable.EveryUpdate(UnityFrameProvider.FixedUpdate)
        .Where(_  => this != null)
        .Subscribe(_ => 
        {
            StateMachine.OnLogic();
            this.IsGrounded();
        })
        .AddToPool(this);
    }

    public virtual void Get() { }

    public virtual void Release() { }

    protected virtual void RegisterStates(StateMachine<CharacterStateType> fsm) 
        => fsm.AddState(CharacterStateType.Idle, new IdleState(this));

    protected virtual void RegisterTransitions(StateMachine<CharacterStateType> fsm) { }

    protected virtual void CacheComponents()
    {
        Renderer = this.FindChildAssert<SpriteRenderer>(recursive: true);
        Animator = Renderer?.GetComponentAssert<Animator>();
        Collider = this.GetComponentAssert<Collider2D>();
    }

    public virtual void RelocateTo(Spawnpoint targetSpawn)
    {
        if (targetSpawn == null || Collider == null) 
            return;

        Vector3 spawnPosition = targetSpawn.transform.position;
        spawnPosition.y -= Collider.offset.y;
        transform.position = spawnPosition;
        transform.rotation = targetSpawn.transform.rotation;
    }
}
