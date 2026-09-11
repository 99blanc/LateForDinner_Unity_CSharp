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
                AttributeView<short> sView => new AttributeSaveData { Key = key, DataType = Literal.Types.Short, BaseValue = sView.BaseValue.Value.ToString(), CurrentValue = sView.CurrentValue.Value.ToString() },
                AttributeView<int> iView => new AttributeSaveData { Key = key, DataType = Literal.Types.Int, BaseValue = iView.BaseValue.Value.ToString(), CurrentValue = iView.CurrentValue.Value.ToString() },
                AttributeView<long> lView => new AttributeSaveData { Key = key, DataType = Literal.Types.Long, BaseValue = lView.BaseValue.Value.ToString(), CurrentValue = lView.CurrentValue.Value.ToString() },
                AttributeView<float> fView => new AttributeSaveData { Key = key, DataType = Literal.Types.Float, BaseValue = fView.BaseValue.Value.ToString(), CurrentValue = fView.CurrentValue.Value.ToString() },
                AttributeView<double> dView => new AttributeSaveData { Key = key, DataType = Literal.Types.Double, BaseValue = dView.BaseValue.Value.ToString(), CurrentValue = dView.CurrentValue.Value.ToString() },
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
                    if (short.TryParse(data.BaseValue, out var sBase) && short.TryParse(data.CurrentValue, out var sVal))
                    {
                        SetBase(attributeType, sBase);
                        Set(attributeType, sVal);
                    }
                    break;
                case Literal.Types.Int:
                    if (int.TryParse(data.BaseValue, out var iBase) && int.TryParse(data.CurrentValue, out var iVal))
                    {
                        SetBase(attributeType, iBase);
                        Set(attributeType, iVal);
                    }
                    break;
                case Literal.Types.Long:
                    if (long.TryParse(data.BaseValue, out var lBase) && long.TryParse(data.CurrentValue, out var lVal))
                    {
                        SetBase(attributeType, lBase);
                        Set(attributeType, lVal);
                    }
                    break;
                case Literal.Types.Float:
                    if (float.TryParse(data.BaseValue, out var fBase) && float.TryParse(data.CurrentValue, out var fVal))
                    {
                        SetBase(attributeType, fBase);
                        Set(attributeType, fVal);
                    }
                    break;
                case Literal.Types.Double:
                    if (double.TryParse(data.BaseValue, out var dBase) && double.TryParse(data.CurrentValue, out var dVal))
                    {
                        SetBase(attributeType, dBase);
                        Set(attributeType, dVal);
                    }
                    break;
                default:
                    Log.Warning(Managers.Localization.Get(LocalizationKey.Log_Attribute_Registry_Unsupported, data.Key, data.DataType));
                    break;
            }
        }
    }

    public void AddBaseAttributeValue(AttributeType dataType, float delta)
    {
        if (!_attributes.TryGetValue(dataType, out var view))
            return;

        switch (view)
        {
            case AttributeView<float> fView:
                float newBaseF = fView.BaseValue.Value + delta;
                SetBase(dataType, newBaseF);

                if (fView.CurrentValue.Value > newBaseF)
                    Set(dataType, newBaseF);
                break;
            case AttributeView<int> iView:
                int iDelta = (int)delta;
                int newBaseI = iView.BaseValue.Value + iDelta;
                SetBase(dataType, newBaseI);

                if (iView.CurrentValue.Value > newBaseI)
                    Set(dataType, newBaseI);
                break;
            case AttributeView<short> sView:
                short sDelta = (short)delta;
                short newBaseS = (short)(sView.BaseValue.Value + sDelta);
                SetBase(dataType, newBaseS);

                if (sView.CurrentValue.Value > newBaseS)
                    Set(dataType, newBaseS);
                break;
            case AttributeView<long> lView:
                long lDelta = (long)delta;
                long newBaseL = lView.BaseValue.Value + lDelta;
                SetBase(dataType, newBaseL);

                if (lView.CurrentValue.Value > newBaseL)
                    Set(dataType, newBaseL);
                break;
            case AttributeView<double> dView:
                double dDelta = (double)delta;
                double newBaseD = dView.BaseValue.Value + dDelta;
                SetBase(dataType, newBaseD);

                if (dView.CurrentValue.Value > newBaseD)
                    Set(dataType, newBaseD);
                break;
        }
    }

    public void AddAttributeValue(AttributeType dataType, float delta)
    {
        if (!_attributes.TryGetValue(dataType, out var view))
            return;

        switch (view)
        {
            case AttributeView<float> fView:
                Set(dataType, fView.CurrentValue.Value + delta);
                break;
            case AttributeView<int> iView:
                int iDelta = (int)delta;
                Set(dataType, iView.CurrentValue.Value + iDelta);
                break;
            case AttributeView<short> sView:
                short sDelta = (short)delta;
                Set(dataType, (short)(sView.CurrentValue.Value + sDelta));
                break;
            case AttributeView<long> lView:
                long lDelta = (long)delta;
                Set(dataType, lView.CurrentValue.Value + lDelta);
                break;
            case AttributeView<double> dView:
                double dDelta = (double)delta;
                Set(dataType, dView.CurrentValue.Value + dDelta);
                break;
        }
    }

    public IEnumerable<AttributeType> GetRegisteredAttributeTypes()
        => _attributes.Keys;
}
