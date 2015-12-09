using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace WB.Core.GenericSubdomains.Portable
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
            where TValue : new()
        {
            return dictionary.GetOrAdd(key, () => new TValue());
        }

        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> valueInit)
        {
            TValue value;

            if (dict.TryGetValue(key, out value))
                return value;

            value = valueInit.Invoke();
            dict.Add(key, value);
            return value;
        }

        public static TValue GetOrNull<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
            where TValue : class
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : null;
        }

        public static TValue? GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
            where TValue : struct
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : null as TValue?;
        }

        public static TValue GetOrNull<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key)
            where TValue : class
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : null;
        }
    }
}