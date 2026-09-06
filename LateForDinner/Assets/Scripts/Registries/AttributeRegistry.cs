using LateForDinner.Data;
using R3;
using System;
using System.Collections.Generic;

public class AttributeRegistry
{
    private readonly Dictionary<AttributeType, IAttributeView> _attributes = new Dictionary<AttributeType, IAttributeView>();

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

    public ReactiveProperty<T> Get<T>(AttributeType dataType, T value = default) where T : struct
        => GetView(dataType, value).CurrentValue;

    public void Set<T>(AttributeType dataType, T value) where T : struct
    {
        var clampedValue = ClampToBasePath(dataType, value);
        Get<T>(dataType).Value = clampedValue;
    }

    public ReactiveProperty<T> GetBase<T>(AttributeType dataType, T value = default) where T : struct
        => GetView(dataType, value).BaseValue;

    public void SetBase<T>(AttributeType dataType, T baseValue) where T : struct
        => GetBase<T>(dataType).Value = baseValue;

    private T ClampToBasePath<T>(AttributeType dataType, T value) where T : struct
    {
        if (!_attributes.TryGetValue(dataType, out var view))
            return value;

        return view switch
        {
            AttributeView<short> sView => (T)(object)Math.Clamp((short)(object)value, (short)0, sView.BaseValue.Value),
            AttributeView<int> iView => (T)(object)Math.Clamp((int)(object)value, (int)0, iView.BaseValue.Value),
            AttributeView<long> lView => (T)(object)Math.Clamp((long)(object)value, (long)0, lView.BaseValue.Value),
            AttributeView<float> fView => (T)(object)Math.Clamp((float)(object)value, (float)0, fView.BaseValue.Value),
            AttributeView<double> dView => (T)(object)Math.Clamp((double)(object)value, (double)0, dView.BaseValue.Value),
            _ => value
        };
    }

    public List<AttributeSaveData> ExportSaveData()
    {
        var list = new List<AttributeSaveData>();

        foreach (var pair in _attributes)
        {
            string key = pair.Key.ToString();
            AttributeSaveData saveData = pair.Value switch
            {
                AttributeView<short> sView => new AttributeSaveData { Key = key, DataType = Literal.Types.Short, Value = sView.CurrentValue.Value.ToString() },
                AttributeView<int> iView => new AttributeSaveData { Key = key, DataType = Literal.Types.Int, Value = iView.CurrentValue.Value.ToString() },
                AttributeView<long> lView => new AttributeSaveData { Key = key, DataType = Literal.Types.Long, Value = lView.CurrentValue.Value.ToString() },
                AttributeView<float> fView => new AttributeSaveData { Key = key, DataType = Literal.Types.Float, Value = fView.CurrentValue.Value.ToString() },
                AttributeView<double> dView => new AttributeSaveData { Key = key, DataType = Literal.Types.Double, Value = dView.CurrentValue.Value.ToString() },
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
                    if (short.TryParse(data.Value, out var sVal))
                    {
                        SetBase(attributeType, sVal);
                        Set(attributeType, sVal);
                    }
                    break;
                case Literal.Types.Int:
                    if (int.TryParse(data.Value, out var iVal))
                    {
                        SetBase(attributeType, iVal);
                        Set(attributeType, iVal);
                    }
                    break;
                case Literal.Types.Long:
                    if (long.TryParse(data.Value, out var lVal))
                    {
                        SetBase(attributeType, lVal);
                        Set(attributeType, lVal);
                    }
                    break;
                case Literal.Types.Float:
                    if (float.TryParse(data.Value, out var fVal))
                    {
                        SetBase(attributeType, fVal);
                        Set(attributeType, fVal);
                    }
                    break;
                case Literal.Types.Double:
                    if (double.TryParse(data.Value, out var dVal))
                    {
                        SetBase(attributeType, dVal);
                        Set(attributeType, dVal);
                    }
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
