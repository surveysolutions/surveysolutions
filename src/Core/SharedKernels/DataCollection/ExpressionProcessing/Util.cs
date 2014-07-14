using System;
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

        public static string GetRosterStringKey(Identity[] scopeIds)
        {
            return String.Join("$", scopeIds.Select(ConversionHelper.ConvertIdentityToString));
        }

        public static Identity[] GetRosterKey(Guid[] rosterScopeIds, decimal[] rosterVector)
        {
            return rosterScopeIds.Select(x => new Identity(x, rosterVector)).ToArray();
        }

        public static string GetSiblingsKey(Guid[] rosterScopeIds)
        {
            return String.Join("$", rosterScopeIds);
        }
    }
}