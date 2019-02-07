using System.Collections.Generic;
using System.Linq;

namespace WB.Services.Export.Events
{
    public static class DictionaryExtensions
    {
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> items)
            => items.ToDictionary(item => item.Key, item => item.Value);
    }
}
