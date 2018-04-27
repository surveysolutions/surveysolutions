using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.DataCollection
{
    public static class Util
    {
        public static T[] Shrink<T>(this IEnumerable<T> vector)
        {
            var enumerable = vector as T[] ?? vector.ToArray();
            return enumerable.Take(enumerable.Length - 1).ToArray();
        }

        public static decimal[] EmptyRosterVector = new decimal[0];

        public static decimal[] GetRosterVector(decimal[] outerRosterVector, decimal rosterInstanceId)
        {
            return outerRosterVector.ExtendWithOneItem(rosterInstanceId);
        }

        public static int[] GetRosterVector(int[] outerRosterVector, int rosterInstanceId)
        {
            return outerRosterVector.ExtendWithOneItem(rosterInstanceId);
        }

        public static string GetRosterStringKey(Identity[] scopeIds)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var scopeId in scopeIds)
            {
                builder.Append("$");
                builder.Append(scopeId.Id.ToString());

                foreach (var coordinate in scopeId.RosterVector.Coordinates)
                {
                    builder.Append("-");

                    builder.Append(Convert.ToInt32(coordinate));
                }
            }

            builder.Append("|");
            return builder.ToString();
        }

        public static Identity[] GetRosterKey(Guid[] rosterScopeIds, decimal[] rosterVector)
        {
            return rosterScopeIds.Select((t, i) => new Identity(t, rosterVector.Take(i + 1).ToArray())).ToArray();
        }

        public static string GetSiblingsKey(Identity[] rosterKey)
        {
            var parentRosterKey = rosterKey.Shrink();
            return GetSiblingsKey(parentRosterKey, rosterKey.Last().Id);
        }


        public static string GetSiblingsKey(Identity[] parentRosterKey, Guid scopeId)
        {
            var parentStringKey = GetRosterStringKey(parentRosterKey);

            return String.IsNullOrEmpty(parentStringKey)
                ? String.Format("{0:N}", scopeId)
                : String.Join("$", GetRosterStringKey(parentRosterKey), String.Format("{0:N}", scopeId));
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

        public static int[] ParseMinusDelimitedIntArray(this string arrayString)
        {
            if (string.IsNullOrWhiteSpace(arrayString) || string.IsNullOrWhiteSpace(arrayString.Trim('_'))) return null;

            //"-1-2--3".Split('-') => string[5] { "", "1", "2", "", "3" }
            // every empty space mean that we encounter negative number
            var items = arrayString.Split('-');
            var result = new List<int>();

            for (int i = 0; i < items.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(items[i]))
                {
                    if (i == items.Length - 1) // if this is last item in array
                        continue;

                    // parse next item and increment index
                    result.Add(-int.Parse(items[++i]));
                }
                else
                    result.Add(int.Parse(items[i]));
            }

            return result.ToArray();
        }
    }
}
