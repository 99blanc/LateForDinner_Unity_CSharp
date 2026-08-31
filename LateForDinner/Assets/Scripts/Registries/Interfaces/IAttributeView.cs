using R3;

public interface IAttributeView { }

public class AttributeView<T> : IAttributeView where T : struct
{
    public readonly ReactiveProperty<T> BaseValue;
    public readonly ReactiveProperty<T> CurrentValue;

    public AttributeView(T defaultValue)
    {
        BaseValue = new ReactiveProperty<T>(defaultValue);
        CurrentValue = new ReactiveProperty<T>(defaultValue);
    }
}