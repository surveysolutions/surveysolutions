﻿using System;
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
    }
}