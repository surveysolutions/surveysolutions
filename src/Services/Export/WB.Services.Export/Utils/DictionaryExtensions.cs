using System.Collections.Generic;
using System.Text;


namespace WB.Services.Export.Utils
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrNull<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
            where TValue : class
        {
            return dictionary.TryGetValue(key, out var value) ? value : null;
        }
    }
}
