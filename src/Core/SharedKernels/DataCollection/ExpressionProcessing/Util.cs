using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.ExpressionProcessing
{
    public static class Util
    {
        public static T[] Shrink<T>(this IEnumerable<T> vector)
        {
            var enumerable = vector as T[] ?? vector.ToArray();
            return enumerable.Take(enumerable.Count() - 1).ToArray();
        }

        public static decimal[] EmptyRosterVector = new decimal[0];

        public static decimal[] GetRosterVector(decimal[] outerRosterVector, decimal rosterInstanceId)
        {
            var outerRosterList = outerRosterVector.ToList();
            outerRosterList.Add(rosterInstanceId);
            return outerRosterList.ToArray();
        }
    }
}