using LateForDinner.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class AttributeExtensions
{
    private static readonly Dictionary<AttributeType, Type> _attributes = new Dictionary<AttributeType, Type>();

    public static Dictionary<string, AttributeData> BindTypes(this Dictionary<string, AttributeData> attributes)
    {
        if (attributes == null)
            return attributes;

        foreach (var data in attributes.Values)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.Key))
                continue;

            if (Enum.TryParse<AttributeType>(data.Key, out var attributeType))
                RegisterType(attributeType, data.DataType);
        }

        return attributes;
    }

    public static void RegisterType(AttributeType attributeType, string dataType)
    {
        Type targetType = dataType?.ToLowerInvariant() switch
        {
            Literal.Types.Short => typeof(short),
            Literal.Types.Int => typeof(int),
            Literal.Types.Long => typeof(long),
            Literal.Types.Float => typeof(float),
            Literal.Types.Double => typeof(double),
            _ => typeof(short)
        };

        _attributes[attributeType] = targetType;
    }

    public static Type GetValueType(this AttributeType attributeType)
    {
        if (_attributes.TryGetValue(attributeType, out var type))
            return type;

        return typeof(short);
    }

    public static object ParseValue(this AttributeType attributeType, string value)
    {
        var targetType = attributeType.GetValueType();

        if (targetType == typeof(float))
            return float.TryParse(value, out var val) ? val : default(float);

        if (targetType == typeof(short))
            return short.TryParse(value, out var val) ? val : default(short);

        if (targetType == typeof(int))
            return int.TryParse(value, out var val) ? val : default(int);

        if (targetType == typeof(long))
            return long.TryParse(value, out var val) ? val : default(long);

        if (targetType == typeof(double))
            return double.TryParse(value, out var val) ? val : default(double);

        return null;
    }

    public static void SetParsedValue(this AttributeRegistry attributes, AttributeType attributeType, string value)
    {
        object parsedValue = attributeType.ParseValue(value);

        if (parsedValue == null)
            return;

        string keyStr = attributeType.ToString();
        double maxLimit = 0.0;
        bool hasMaxLimit = Managers.Data.Attributes.TryGetValue(keyStr, out var attrData) && attrData.MaxValue > 0f;

        if (hasMaxLimit)
            maxLimit = attrData.MaxValue;

        double finalValue = parsedValue switch
        {
            float val => hasMaxLimit ? Math.Clamp(val, 0.0, maxLimit) : Math.Max(0.0, val),
            int val => hasMaxLimit ? Math.Clamp(val, 0.0, maxLimit) : Math.Max(0.0, val),
            short val => hasMaxLimit ? Math.Clamp(val, 0.0, maxLimit) : Math.Max(0.0, val),
            long val => hasMaxLimit ? Math.Clamp(val, 0.0, maxLimit) : Math.Max(0.0, val),
            double val => hasMaxLimit ? Math.Clamp(val, 0.0, maxLimit) : Math.Max(0.0, val),
            _ => 0.0
        };

        switch (parsedValue)
        {
            case float: 
                Set((float)finalValue); 
                break;
            case int: 
                Set((int)finalValue); 
                break;
            case short: 
                Set((short)finalValue); 
                break;
            case long: 
                Set((long)finalValue); 
                break;
            case double: 
                Set(finalValue); 
                break;
        }

        void Set<T>(T v) where T : struct
            => attributes.Set(attributeType, v);
    }

    public static void SetBaseParsedValue(this AttributeRegistry attributes, AttributeType attributeType, string value)
    {
        object parsedValue = attributeType.ParseValue(value);

        if (parsedValue == null)
            return;

        string keyStr = attributeType.ToString();
        double maxLimit = 0.0;
        bool hasMaxLimit = Managers.Data.Attributes.TryGetValue(keyStr, out var attrData) && attrData.MaxValue > 0f;

        if (hasMaxLimit)
            maxLimit = attrData.MaxValue;

        double finalValue = parsedValue switch
        {
            float val => hasMaxLimit ? Math.Clamp(val, 0.0, maxLimit) : Math.Max(0.0, val),
            int val => hasMaxLimit ? Math.Clamp(val, 0.0, maxLimit) : Math.Max(0.0, val),
            short val => hasMaxLimit ? Math.Clamp(val, 0.0, maxLimit) : Math.Max(0.0, val),
            long val => hasMaxLimit ? Math.Clamp(val, 0.0, maxLimit) : Math.Max(0.0, val),
            double val => hasMaxLimit ? Math.Clamp(val, 0.0, maxLimit) : Math.Max(0.0, val),
            _ => 0.0
        };

        switch (parsedValue)
        {
            case float:
                Set((float)finalValue);
                break;
            case int:
                Set((int)finalValue);
                break;
            case short:
                Set((short)finalValue);
                break;
            case long:
                Set((long)finalValue);
                break;
            case double:
                Set(finalValue);
                break;
        }

        void Set<T>(T v) where T : struct
            => attributes.SetBase(attributeType, v);
    }

    public static string GetParsedValueString(this AttributeRegistry attributes, AttributeType attributeType)
    {
        var targetType = attributeType.GetValueType();

        if (targetType == typeof(float))
            return attributes.Get(attributeType, default(float)).Value.ToString();

        if (targetType == typeof(short))
            return attributes.Get(attributeType, default(short)).Value.ToString();

        if (targetType == typeof(int))
            return attributes.Get(attributeType, default(int)).Value.ToString();

        if (targetType == typeof(long))
            return attributes.Get(attributeType, default(long)).Value.ToString();

        if (targetType == typeof(double))
            return attributes.Get(attributeType, default(double)).Value.ToString();

        return null;
    }

    public static string GetParsedBaseValueString(this AttributeRegistry attributes, AttributeType attributeType)
    {
        var targetType = attributeType.GetValueType();

        if (targetType == typeof(float))
            return attributes.GetBase(attributeType, default(float)).Value.ToString();

        if (targetType == typeof(short))
            return attributes.GetBase(attributeType, default(short)).Value.ToString();

        if (targetType == typeof(int))
            return attributes.GetBase(attributeType, default(int)).Value.ToString();

        if (targetType == typeof(long))
            return attributes.GetBase(attributeType, default(long)).Value.ToString();

        if (targetType == typeof(double))
            return attributes.GetBase(attributeType, default(double)).Value.ToString();

        return null;
    }
}
