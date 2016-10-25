using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection
{
    public static class Util
    {
        public static IEnumerable<T> GetCommonPart<T>(this IEnumerable<T> vector1, IEnumerable<T> vector2)
        {
            var array1 = vector1 as T[] ?? vector1.ToArray();
            var array2 = vector2 as T[] ?? vector2.ToArray();

            var minLength = Math.Min(array1.Count(), array2.Count());
            for (int i = 0; i < minLength; i++)
            {
                if (array1[i].Equals(array2[i]))
                {
                    yield return array1[i];
                }
                else
                {
                    yield break;
                }
            }
        }

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
            return String.Join("$", scopeIds.Select(ConversionHelper.ConvertIdentityToString).ToArray());
        }

        public static Identity[] GetRosterKey(Guid[] rosterScopeIds, decimal[] rosterVector)
        {
            return rosterScopeIds.Select((t, i) => new Identity(t, rosterVector.Take(i + 1).ToArray())).ToArray();
        }

        public static string GetSiblingsKey(Identity[] rosterKey)
        {
            var parentRosterKey = rosterKey.Shrink();//.Select(x => new Identity(x.Id, x.RosterVector.Shrink())).ToArray();
            return GetSiblingsKey(parentRosterKey, rosterKey.Last().Id);
        }


        public static string GetSiblingsKey(Identity[] parentRosterKey, Guid scopeId)
        {
            var parentStringKey = GetRosterStringKey(parentRosterKey);

            return string.IsNullOrEmpty(parentStringKey)
                ? string.Format("{0:N}", scopeId)
                : String.Join("$", GetRosterStringKey(parentRosterKey), string.Format("{0:N}", scopeId));
        }

        //------

        public static string GetSiblingsKey(Guid[] rosterScopeIds)
        {
            return String.Join("$", rosterScopeIds);
        }

        public static string GetSiblingsKey(Guid[] rosterScopeIds, Guid scopeId)
        {
            return String.Join("$", GetSiblingsKey(rosterScopeIds), scopeId);
        }
    }
}