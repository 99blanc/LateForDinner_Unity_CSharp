using R3;
using System;
using System.Collections.Generic;
using LateForDinner.Data;

public class AttributeRegistry
{
    private readonly Dictionary<AttributeType, IAttributeView> _attributes = new Dictionary<AttributeType, IAttributeView>();

    public void InitAttribute(List<AttributeData> attributes)
    {
        foreach (var data in attributes)
        {
            if (!Enum.TryParse<AttributeType>(data.Key, out var attributeType))
                continue;

            switch (data.DataType?.ToLowerInvariant())
            {
                case Literal.Types.Short:
                    Get<short>(attributeType, default);
                    break;
                case Literal.Types.Int:
                    Get<int>(attributeType, default);
                    break;
                case Literal.Types.Long:
                    Get<long>(attributeType, default);
                    break;
                case Literal.Types.Float:
                    Get<float>(attributeType, default);
                    break;
                case Literal.Types.Double:
                    Get<double>(attributeType, default);
                    break;
                default:
                    Log.Warning(Managers.Localization.Get(LocalizationKey.Log_Attribute_Registry_Unsupported, data.Key, data.DataType));
                    break;
            }
        }
    }

    public object GetValue(AttributeType attributeType)
    {
        Type valueType = attributeType.GetValueType();
        return valueType switch
        {
            var t when t == typeof(float) => Get<float>(attributeType).Value,
            var t when t == typeof(int) => Get<int>(attributeType).Value,
            var t when t == typeof(short) => Get<short>(attributeType).Value,
            var t when t == typeof(long) => Get<long>(attributeType).Value,
            var t when t == typeof(double) => Get<double>(attributeType).Value,
            _ => Get<short>(attributeType).Value
        };
    }

    public ReadOnlyReactiveProperty<T> Stream<T>(AttributeType dataType) where T : struct
        => GetView<T>(dataType).CurrentValue;

    public ReactiveProperty<T> Get<T>(AttributeType dataType, T value = default) where T : struct
        => GetView<T>(dataType, value).CurrentValue;

    private AttributeView<T> GetView<T>(AttributeType dataType, T value = default) where T : struct
    {
        if (!_attributes.TryGetValue(dataType, out var view))
        {
            var newView = new AttributeView<T>(value);
            _attributes[dataType] = newView;
            return newView;
        }

        return (AttributeView<T>)view;
    }

    public void Set<T>(AttributeType dataType, T value) where T : struct
        => Get<T>(dataType).Value = value;

    public void SetBaseValue<T>(AttributeType dataType, T baseValue) where T : struct
    {
        var view = GetView<T>(dataType);
        view.BaseValue = baseValue;
        view.CurrentValue.Value = baseValue;
    }
}
