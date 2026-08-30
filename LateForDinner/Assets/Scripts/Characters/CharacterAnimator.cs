using UnityEngine;

public abstract class CharacterAnimator : MonoBehaviour
{
    public InteractionType CurrentHoldInteractionType { get; set; } = InteractionType.None;

    public Animator Animator { get; set; }

    public void SetAnimator(Animator animator)
        => Animator = animator;

    public void SetOverrideController(RuntimeAnimatorController overrideController)
    {
        if (Animator != null && overrideController != null)
            Animator.runtimeAnimatorController = overrideController;
    }

    public void SetAnimatorSpeed(float speed)
    {
        if (Animator != null)
            Animator.speed = speed;
    }

    public virtual float GetCurrentAnimatorNormalizedTime()
    {
        if (Animator == null)
            return 0f;

        AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.normalizedTime;
    }

    protected virtual int GetStateHash(CharacterStateType state)
    {
        if (CurrentHoldInteractionType != InteractionType.None)
        {
            int interactionHash = GetInteractionStateHash(state, CurrentHoldInteractionType);

            if (interactionHash != 0)
                return interactionHash;
        }

        return GetDefaultStateHash(state);
    }

    protected virtual int GetDefaultStateHash(CharacterStateType state)
    {
        return state switch
        {
            CharacterStateType.Idle => Define.Animation.Idle,
            CharacterStateType.Move => Define.Animation.Move,
            CharacterStateType.Fall => Define.Animation.Fall,
            CharacterStateType.Jump => Define.Animation.Jump,
            _ => Define.Animation.Idle
        };
    }

    protected virtual int GetInteractionStateHash(CharacterStateType state, InteractionType interactionType)
    {
        return (state, interactionType) switch
        {
            (CharacterStateType.Idle, InteractionType.Tray) => Define.Animation.PickupTrayIdle,
            (CharacterStateType.Move, InteractionType.Tray) => Define.Animation.PickupTrayMove,
            (CharacterStateType.Fall, InteractionType.Tray) => Define.Animation.PickupTrayFall,
            (CharacterStateType.Jump, InteractionType.Tray) => Define.Animation.PickupTrayJump,
            (CharacterStateType.DoubleJump, InteractionType.Tray) => Define.Animation.PickupTrayDoubleJump,
            (CharacterStateType.Throw, InteractionType.Tray) => Define.Animation.ThrowTray,
            _ => 0
        };
    }

    private void Play(int hash)
        => Animator?.Play(hash);

    public virtual void PlayState(CharacterStateType state)
        => Play(GetStateHash(state));
}
