using LateForDinner.Data;
using System;
using System.Collections.Generic;

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
        => _attributes.TryGetValue(attributeType, out var type) ? type : typeof(short);

    public static object ParseValue(this AttributeType attributeType, string value)
    {
        var targetType = attributeType.GetValueType();

        if (targetType == typeof(float)) 
            return float.TryParse(value, out var f) ? f : 0f;

        if (targetType == typeof(int)) 
            return int.TryParse(value, out var i) ? i : 0;

        if (targetType == typeof(short)) 
            return short.TryParse(value, out var s) ? s : (short)0;

        if (targetType == typeof(long)) 
            return long.TryParse(value, out var l) ? l : 0L;

        if (targetType == typeof(double)) 
            return double.TryParse(value, out var d) ? d : 0.0;

        return null;
    }

    private static double ClampAndConvertToType(object parsedValue, AttributeType attributeType)
    {
        string keyStr = attributeType.ToString();
        double maxLimit = Managers.Data.Attributes.TryGetValue(keyStr, out var attrData) ? attrData.MaxValue : (double)0;
        double rawVal = Convert.ToDouble(parsedValue);
        return maxLimit > 0 ? Math.Clamp(rawVal, 0.0, maxLimit) : Math.Max(0.0, rawVal);
    }

    public static void SetParsedValue(this AttributeRegistry attributes, AttributeType attributeType, string value)
    {
        var parsed = attributeType.ParseValue(value);

        if (parsed == null) 
            return;

        double finalVal = ClampAndConvertToType(parsed, attributeType);
        attributes.ApplyTypedSet(attributeType, finalVal, false);
    }

    public static void SetParsedBaseValue(this AttributeRegistry attributes, AttributeType attributeType, string value)
    {
        var parsed = attributeType.ParseValue(value);

        if (parsed == null) 
            return;

        double finalVal = ClampAndConvertToType(parsed, attributeType);
        attributes.ApplyTypedSet(attributeType, finalVal, true);
    }

    private static void ApplyTypedSet(this AttributeRegistry attributes, AttributeType attributeType, double val, bool isBase)
    {
        var targetType = attributeType.GetValueType();

        if (targetType == typeof(int))
        {
            int v = (int)val;

            if (isBase) 
                attributes.SetBase(attributeType, v); 
            else 
                attributes.Set(attributeType, v);
        }
        if (targetType == typeof(float))
        {
            float v = (float)val;

            if (isBase) 
                attributes.SetBase(attributeType, v); 
            else 
                attributes.Set(attributeType, v);
        }
        if (targetType == typeof(short))
        {
            short v = (short)val;

            if (isBase) 
                attributes.SetBase(attributeType, v); 
            else 
                attributes.Set(attributeType, v);
        }
        if (targetType == typeof(long))
        {
            long v = (long)val;

            if (isBase) 
                attributes.SetBase(attributeType, v); 
            else 
                attributes.Set(attributeType, v);
        }
        if (targetType == typeof(double))
        {
            if (isBase) 
                attributes.SetBase(attributeType, val);
            else 
                attributes.Set(attributeType, val);
        }
    }

    public static void AddValue(this AttributeRegistry attributes, AttributeType attributeType, string valueStr)
    {
        var parsed = attributeType.ParseValue(valueStr);

        if (parsed == null) 
            return;

        double current = attributeType.GetCurrentDoubleValue(attributes);
        double add = Convert.ToDouble(parsed);
        attributes.SetParsedValue(attributeType, (current + add).ToString());
    }

    public static void SubValue(this AttributeRegistry attributes, AttributeType attributeType, string valueStr)
    {
        var parsed = attributeType.ParseValue(valueStr);

        if (parsed == null) 
            return;

        double current = attributeType.GetCurrentDoubleValue(attributes);
        double sub = Convert.ToDouble(parsed);
        attributes.SetParsedValue(attributeType, (current - sub).ToString());
    }

    public static void AddBaseValue(this AttributeRegistry attributes, AttributeType attributeType, string valueStr)
    {
        var parsed = attributeType.ParseValue(valueStr);

        if (parsed == null) 
            return;

        double current = attributes.GetBaseDoubleValue(attributeType);
        double add = Convert.ToDouble(parsed);
        attributes.SetParsedBaseValue(attributeType, (current + add).ToString());
    }

    public static void SubBaseValue(this AttributeRegistry attributes, AttributeType attributeType, string valueStr)
    {
        var parsed = attributeType.ParseValue(valueStr);

        if (parsed == null) 
            return;

        double current = attributes.GetBaseDoubleValue(attributeType);
        double sub = Convert.ToDouble(parsed);
        attributes.SetParsedBaseValue(attributeType, (current - sub).ToString());
    }

    public static double GetCurrentDoubleValue(this AttributeType attributeType, AttributeRegistry attributes)
    {
        var type = attributeType.GetValueType();

        if (type == typeof(int))
            return attributes.Get(attributeType, (int)0).Value;

        if (type == typeof(float))
            return attributes.Get(attributeType, (float)0).Value;

        if (type == typeof(short))
            return attributes.Get(attributeType, (short)0).Value;

        if (type == typeof(long))
            return attributes.Get(attributeType, (long)0).Value;

        if (type == typeof(double))
            return attributes.Get(attributeType, (double)0).Value;

        return 0.0;
    }

    public static double GetBaseDoubleValue(this AttributeRegistry attributes, AttributeType attributeType)
    {
        var type = attributeType.GetValueType();

        if (type == typeof(int)) 
            return attributes.GetBase(attributeType, (int)0).Value;

        if (type == typeof(float)) 
            return attributes.GetBase(attributeType, (float)0).Value;

        if (type == typeof(short)) 
            return attributes.GetBase(attributeType, (short)0).Value;

        if (type == typeof(long)) 
            return attributes.GetBase(attributeType, (long)0).Value;

        if (type == typeof(double)) 
            return attributes.GetBase(attributeType, (double)0).Value;

        return 0.0;
    }

    public static string GetParsedValueString(this AttributeRegistry attributes, AttributeType attributeType)
        => attributeType.GetCurrentDoubleValue(attributes).ToString();

    public static string GetParsedBaseValueString(this AttributeRegistry attributes, AttributeType attributeType)
        => attributes.GetBaseDoubleValue(attributeType).ToString();

    public static bool IsUnify(this AttributeType attributeType)
    {
        string key = attributeType.ToString();

        if (Managers.Data.Attributes.TryGetValue(key, out var attributeData))
            return attributeData.Unify;

        return false;
    }

    public static List<AttributeSaveData> CreateDefaultAttributes(this CharacterID characterID)
    {
        var list = new List<AttributeSaveData>();
        int charIndex = (int)characterID;

        if (Managers.Data?.PlayableCharacterTemplates is { } templates && templates.Contains(charIndex))
        {
            foreach (var template in templates[charIndex])
            {
                if (!Enum.TryParse<AttributeType>(template.AttributeKey, out var attributeType))
                    continue;

                string dataType = Literal.Types.Float;
                bool isUnify = false;

                if (Managers.Data.Attributes.TryGetValue(template.AttributeKey, out var data))
                {
                    dataType = data.DataType;
                    isUnify = data.Unify;
                }

                string stringValue = template.Value ?? "0";
                list.Add(new AttributeSaveData()
                {
                    Key = template.AttributeKey,
                    DataType = dataType,
                    BaseValue = stringValue,
                    CurrentValue = stringValue
                });
            }
        }

        return list;
    }
}
