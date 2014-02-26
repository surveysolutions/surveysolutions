using System.Collections.Generic;

namespace WB.Core.GenericSubdomains.Utils
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> ToEnumerable<T>(this T element)
        {
            yield return element;
        }
    }
}