using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests
{
    internal static class EnumerableExtensions
    {
        public static T Second<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Skip(1).First();
        }
    }
}