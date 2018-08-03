using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.GenericSubdomains.Portable
{
    public static class DictionaryExtensions
    {
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> items)
            => items.ToDictionary(item => item.Key, item => item.Value);

        public static void Add<TKey1, TKey2, TValue>(this Dictionary<TKey1, Dictionary<TKey2, TValue>> dictionary, TKey1 key1, TKey2 key2, TValue value)
        {
            dictionary.GetOrAdd(key1).Add(key2, value);
        }

        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
            where TValue : new()
        {
            return dictionary.GetOrAdd(key, () => new TValue());
        }

        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> valueInit)
        {
            if (dict.TryGetValue(key, out var value))
                return value;

            value = valueInit.Invoke();
            dict.Add(key, value);
            return value;
        }

        public static TValue GetOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> valueInit)
        {
            if (dict.TryGetValue(key, out var value))
                return value;

            value = valueInit.Invoke();
            dict[key] = value;
            return value;
        }

        public static TValue GetOrNull<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
            where TValue : class
        {
            return dictionary.TryGetValue(key, out var value) ? value : null;
        }

        public static List<TValue> GetOrEmpty<TKey, TValue>(this IDictionary<TKey, List<TValue>> dictionary, TKey key)
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : new List<TValue>();
        }
    }
}
