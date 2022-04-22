using System;
using System.Collections.Generic;

namespace WB.Services.Export
{
    public static class DictionaryExtensions
    {
        public static TValue? GetOrNull<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
            where TValue : class
        {
            return dictionary.TryGetValue(key, out var value) ? value : null;
        }

        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> valueInit) where TKey : notnull
        {
            if (dict.TryGetValue(key, out var value))
                return value;

            value = valueInit.Invoke();
            dict.Add(key, value);
            return value;
        }
    }

}
