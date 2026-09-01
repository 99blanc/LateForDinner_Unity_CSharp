using LateForDinner.Data;
using R3;
using System;
using System.Collections.Generic;

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

    public ReadOnlyReactiveProperty<T> Stream<T>(AttributeType dataType) where T : struct
        => GetView<T>(dataType).CurrentValue;

    public ReactiveProperty<T> Get<T>(AttributeType dataType, T value = default) where T : struct
        => GetView(dataType, value).CurrentValue;

    public void Set<T>(AttributeType dataType, T value) where T : struct
        => Get<T>(dataType).Value = value;

    public ReadOnlyReactiveProperty<T> StreamBase<T>(AttributeType dataType) where T : struct
        => GetView<T>(dataType).BaseValue;

    public ReactiveProperty<T> GetBase<T>(AttributeType dataType, T value = default) where T : struct
        => GetView(dataType, value).BaseValue;

    public void SetBase<T>(AttributeType dataType, T baseValue) where T : struct
        => GetBase<T>(dataType).Value = baseValue;

    public List<AttributeSaveData> ExportSaveData()
    {
        var list = new List<AttributeSaveData>();

        foreach (var pair in _attributes)
        {
            string keyStr = pair.Key.ToString();
            AttributeSaveData saveData = pair.Value switch
            {
                AttributeView<short> sView => new AttributeSaveData { Key = keyStr, DataType = Literal.Types.Short, Value = sView.CurrentValue.Value },
                AttributeView<int> iView => new AttributeSaveData { Key = keyStr, DataType = Literal.Types.Int, Value = iView.CurrentValue.Value },
                AttributeView<long> lView => new AttributeSaveData { Key = keyStr, DataType = Literal.Types.Long, Value = lView.CurrentValue.Value },
                AttributeView<float> fView => new AttributeSaveData { Key = keyStr, DataType = Literal.Types.Float, Value = fView.CurrentValue.Value },
                AttributeView<double> dView => new AttributeSaveData { Key = keyStr, DataType = Literal.Types.Double, Value = dView.CurrentValue.Value },
                _ => null
            };

            if (saveData != null)
                list.Add(saveData);
        }

        return list;
    }

    public void ImportSaveData(List<AttributeSaveData> savedDataList)
    {
        if (savedDataList == null)
            return;

        foreach (var data in savedDataList)
        {
            if (!Enum.TryParse<AttributeType>(data.Key, out var attributeType))
                continue;

            switch (data.DataType?.ToLowerInvariant())
            {
                case Literal.Types.Short:
                    Set(attributeType, (short)data.Value);
                    break;
                case Literal.Types.Int:
                    Set(attributeType, (int)data.Value);
                    break;
                case Literal.Types.Long:
                    Set(attributeType, (long)data.Value);
                    break;
                case Literal.Types.Float:
                    Set(attributeType, (float)data.Value);
                    break;
                case Literal.Types.Double:
                    Set(attributeType, data.Value);
                    break;
                default:
                    Log.Warning(Managers.Localization.Get(LocalizationKey.Log_Attribute_Registry_Unsupported, data.Key, data.DataType));
                    break;
            }
        }
    }

    public IEnumerable<AttributeType> GetRegisteredAttributeTypes()
        => _attributes.Keys;
}
