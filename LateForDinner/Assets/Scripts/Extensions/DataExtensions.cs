using System;
using System.Collections.Generic;
using System.Linq;
using ZLinq;

public static class DataExtensions
{
    public static Dictionary<int, Dictionary<string, string>> ToNestedDictionary<T>(this List<T> list, Func<T, int> primaryKeySelector, Func<T, string> secondaryKeySelector, Func<T, string> valueSelector)
    {
        if (list == null) 
            return new Dictionary<int, Dictionary<string, string>>();

        return list
        .GroupBy(primaryKeySelector)
        .ToDictionary(group => group.Key, group => group.ToDictionary(secondaryKeySelector, valueSelector));
    }
}