using System.Collections.Generic;
using System.Linq;

namespace WB.Tests.Unit.Designer
{
    internal static class EnumerableExtensions
    {
        public static T Second<T>(this IEnumerable<T> enumerable) => enumerable.Skip(1).First();
    }
}