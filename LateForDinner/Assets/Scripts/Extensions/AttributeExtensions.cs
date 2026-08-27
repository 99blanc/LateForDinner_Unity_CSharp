using System;
using System.Collections.Generic;
using LateForDinner.Data;

public static class AttributeExtensions
{
    private static readonly Dictionary<AttributeType, Type> _attributes = new Dictionary<AttributeType, Type>();

    public static void BindTypes(this List<AttributeData> attributes)
    {
        if (attributes == null) 
            return;

        for (int index = 0; index < attributes.Count; index++)
        {
            var data = attributes[index];

            if (data == null || string.IsNullOrWhiteSpace(data.Key))
                continue;

            if (Enum.TryParse<AttributeType>(data.Key, out var attributeType))
                RegisterType(attributeType, data.DataType);
        }
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

        switch (parsedValue)
        {
            case float val: 
                attributes.Set(attributeType, val); 
                break;
            case int val: 
                attributes.Set(attributeType, val); 
                break;
            case short val: 
                attributes.Set(attributeType, val); 
                break;
            case long val: 
                attributes.Set(attributeType, val); 
                break;
            case double val: 
                attributes.Set(attributeType, val); 
                break;
        }
    }
}
