using System.Runtime.CompilerServices;
using UnityEngine;

public interface ICarriableCharacter
{
    private static readonly ConditionalWeakTable<ICarriableCharacter, CarryStateValue> _carryValues = new ConditionalWeakTable<ICarriableCharacter, CarryStateValue>();
    private class CarryStateValue
    {
        public Prop HeldProp;
        public bool IsHoldingProp = false;
        public bool HasThrown = false;
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
    public bool HasThrown
    {
        get => _carryValues.GetOrCreateValue(this).HasThrown;
        set => _carryValues.GetOrCreateValue(this).HasThrown = value;
    }

    public void PickupProp(Prop prop)
    {
        if (this is not Character character || prop is not IInteractable interactable)
            return;

        character.CurrentHoldInteractionType = interactable.InteractionType;
        character?.StateMachine?.RequestStateChange(CharacterStateType.Idle, forceInstantly: true);
        Managers.Pool.Push(prop, prop.UniqueKey);
        HeldProp = prop;
        IsHoldingProp = true;
        HasThrown = false;
    }

    public void DropPropDirectly()
    {
        if (this is not Character character || HeldProp == null)
            return;

        var (instance, rentHandle) = Managers.Pool.Pop(HeldProp.UniqueKey);

        if (instance != null)
        {
            float dropDirection = character.GetLookDirectionX();
            instance.transform.position = character.transform.position + new Vector3(dropDirection * 0.5f, 0f, 0f);

            if (instance.TryGetComponent<Rigidbody2D>(out var rd))
                rd.linearVelocity = Vector2.zero;
        }

        ResetHoldState(character);
    }

    public void ExecuteThrow(bool isUpPressed)
    {
        if (this is not Character character || HeldProp == null)
            return;

        var (instance, rentHandle) = Managers.Pool.Pop(HeldProp.UniqueKey);

        if (instance != null)
        {
            if (instance.TryGetComponent<Rigidbody2D>(out var rigidbody))
            {
                float throwDirection = character.GetLookDirectionX();

                if (isUpPressed)
                    rigidbody.linearVelocity = new Vector2(throwDirection * 4f, 7f);
                else
                    rigidbody.linearVelocity = new Vector2(throwDirection * 6f, 2f);
            }
        }

        ResetHoldState(character);
    }

    private void ResetHoldState(Character character)
    {
        character.CurrentHoldInteractionType = InteractionType.None;
        HeldProp = null;
        IsHoldingProp = false;
        HasThrown = false;
    }
}
