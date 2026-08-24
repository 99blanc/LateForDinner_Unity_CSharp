using R3;

public interface IAttributeView { }

public class AttributeView<T> : IAttributeView where T : struct
{
    public T BaseValue { get; set; }
    public readonly ReactiveProperty<T> CurrentValue;

    public AttributeView(T defaultValue)
    {
        BaseValue = defaultValue;
        CurrentValue = new ReactiveProperty<T>(defaultValue);
    }
}