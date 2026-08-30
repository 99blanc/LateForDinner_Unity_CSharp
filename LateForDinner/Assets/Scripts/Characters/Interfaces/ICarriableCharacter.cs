using System.Runtime.CompilerServices;
using UnityEngine;

public interface ICarriableCharacter
{
    private static readonly ConditionalWeakTable<ICarriableCharacter, CarryStateValue> _carryValues = new ConditionalWeakTable<ICarriableCharacter, CarryStateValue>();
    private class CarryStateValue
    {
        public Prop HeldProp;
        public bool IsHoldingProp = false;
    }
    public Prop HeldProp
    {
        get => _carryValues.GetOrCreateValue(this).HeldProp;
        set => _carryValues.GetOrCreateValue(this).HeldProp = value;
    }
    public bool IsHoldingProp
    {
        get => _carryValues.GetOrCreateValue(this).IsHoldingProp;
        set => _carryValues.GetOrCreateValue(this).IsHoldingProp = value;
    }

    public void PickupProp(Prop prop)
    {
        if (this is not Character character || prop is not IInteractable interactable)
            return;

        if (character?.CharacterAnimator is PlayableCharacterAnimator playableAnimator)
            playableAnimator.CurrentHoldInteractionType = interactable.InteractionType;

        character?.StateMachine?.RequestStateChange(CharacterStateType.Idle, forceInstantly: true);
        Managers.Pool.Push(prop);
        HeldProp = prop;
        IsHoldingProp = true;
    }

    public void ThrowProp()
    {
        if (this is not Character character)
            return;

        if (character?.CharacterAnimator is PlayableCharacterAnimator playableAnimator)
            playableAnimator.CurrentHoldInteractionType = InteractionType.None;

        string poolKey = HeldProp.GetType().Name;
        var (instance, rentHandle) = Managers.Pool.Pop(poolKey);

        if (instance == null)
            return;

        var rd = instance.GetComponentAssert<Rigidbody2D>();
        float throwDirection = character.GetLookDirectionX();
        rd.linearVelocity = new Vector2(throwDirection * 5f, 3f);
        HeldProp = null;
        IsHoldingProp = false;
    }
}
