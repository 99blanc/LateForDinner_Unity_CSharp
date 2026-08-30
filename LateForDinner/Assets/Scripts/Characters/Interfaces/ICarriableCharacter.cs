using System.Runtime.CompilerServices;

public interface ICarriableCharacter
{
    private static readonly ConditionalWeakTable<ICarriableCharacter, CarryStateValue> _carryValues = new ConditionalWeakTable<ICarriableCharacter, CarryStateValue>();
    private class CarryStateValue
    {
        public bool IsHoldingProp = false;
    }

    public bool IsHoldingProp
    {
        get => _carryValues.GetOrCreateValue(this).IsHoldingProp;
        set => _carryValues.GetOrCreateValue(this).IsHoldingProp = value;
    }
}
