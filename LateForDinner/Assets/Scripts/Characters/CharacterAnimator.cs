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

    public virtual float GetCurrentAnimatorNormalizedTime()
    {
        if (Animator == null)
            return 0f;

        AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.normalizedTime;
    }

    protected void Play(int hash)
        => Animator?.Play(hash);

    protected virtual int GetStateHash(CharacterStateType state)
    {
        return state switch
        {
            CharacterStateType.Idle => Define.Animation.Idle,
            CharacterStateType.Move => Define.Animation.Move,
            CharacterStateType.Fall => Define.Animation.Fall,
            CharacterStateType.Jump => Define.Animation.Jump,
            CharacterStateType.Roll => Define.Animation.Roll,
            CharacterStateType.Dash => Define.Animation.Dash,
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
    public virtual void PlayRoll()
        => Play(Define.Animation.Roll);
    public virtual void PlayDash()
        => Play(Define.Animation.Dash);
}
