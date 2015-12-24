using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.GenericSubdomains.Portable
{
    public static class DictionaryExtensions
    {
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
            TValue value;

            if (dict.TryGetValue(key, out value))
                return value;

            value = valueInit.Invoke();
            dict.Add(key, value);
            return value;
        }

        public static TValue GetOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> valueInit)
        {
            TValue value;

            if (dict.TryGetValue(key, out value))
                return value;

            value = valueInit.Invoke();
            dict[key] = value;
            return value;
        }

        public static TValue GetOrNull<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
            where TValue : class
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : null;
        }

        public static TValue? GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
            where TValue : struct
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : null as TValue?;
        }

        public static bool TryRemove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue _;
            return dictionary.TryRemove(key, out _);
        }

        public static ConcurrentDictionary<TKey, TElement> ToConcurrentDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector)
        {
            return new ConcurrentDictionary<TKey, TElement>(source.ToDictionary(keySelector, elementSelector));
        }
    }
}