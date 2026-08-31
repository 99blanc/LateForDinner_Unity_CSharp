using UnityEngine;

public abstract class CharacterAnimator : MonoBehaviour
{
    public Character Owner { get; private set; }
    public Animator Animator { get; set; }

    public void SetOwner(Character owner)
        => Owner = owner;

    public void SetAnimator(Animator animator)
        => Animator = animator;

    public void SetOverrideController(RuntimeAnimatorController overrideController)
    {
        if (Animator != null && overrideController != null)
            Animator.runtimeAnimatorController = overrideController;
    }

    protected virtual int GetStateHash(CharacterStateType state)
    {
        InteractionType currentHoldType = Owner != null ? Owner.CurrentHoldInteractionType : InteractionType.None;

        if (currentHoldType != InteractionType.None)
        {
            int interactionHash = GetInteractionStateHash(state, currentHoldType);

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
            (CharacterStateType.Throw, InteractionType.Tray) => Define.Animation.ThrowTray,
            _ => 0
        };
    }

    private void Play(int hash)
    {
        if (Animator == null)
            return;
        
        Animator.Play(hash, 0, 0f);
    }

    public virtual void PlayState(CharacterStateType state)
        => Play(GetStateHash(state));
}
