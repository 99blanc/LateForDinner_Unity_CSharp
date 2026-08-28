using UnityEngine;

public abstract class CharacterAnimator : MonoBehaviour
{
    public Animator Animator { get; set; }

    public void SetAnimator(Animator animator)
        => Animator = animator;

    public void SetOverrideController(RuntimeAnimatorController overrideController)
    {
        if (Animator != null && overrideController != null)
            Animator.runtimeAnimatorController = overrideController;
    }

    protected void Play(int hash)
        => Animator?.Play(hash);

    protected virtual int GetStateHash(CharacterStateType state)
    {
        return state switch
        {
            CharacterStateType.Idle => Define.Animation.Idle,
            CharacterStateType.Move => Define.Animation.Move,
            CharacterStateType.Jump => Define.Animation.Jump,
            _ => Define.Animation.Idle
        };
    }

    public virtual void PlayState(CharacterStateType state)
        => Play(GetStateHash(state));

    public virtual void PlayIdle() 
        => Play(Define.Animation.Idle);
    public virtual void PlayMove() 
        => Play(Define.Animation.Move);
    public virtual void PlayFall()
        => Play(Define.Animation.Fall);
    public virtual void PlayJump()
        => Play(Define.Animation.Jump);
}
