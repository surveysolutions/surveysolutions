using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.Implementation
{
    internal static class EnumerableExtensions
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }
    }
}