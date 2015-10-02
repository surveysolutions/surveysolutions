using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.Utils
{
    public static class CuncurrentDictionaryExtensions
    {
        public static void Remove<T, V>(this ConcurrentDictionary<T, V> dictionary, T key)
        {
            V value;
            dictionary.TryRemove(key, out value);
        }

        public static ConcurrentDictionary<TKey, TElement> ToConcurrentDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector)
        {
            return new ConcurrentDictionary<TKey, TElement>(source.ToDictionary(keySelector, elementSelector));
        }
    }
}