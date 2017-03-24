using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Utils
{
    public class RosterVectorAsCoordinatesComparer : IComparer<RosterVector>
    {
        public static RosterVectorAsCoordinatesComparer Instance = new RosterVectorAsCoordinatesComparer();

        public int Compare(RosterVector x, RosterVector y)
        {
            for (int idx = 0; idx < Math.Max(x.Length, y.Length); idx++)
            {
                var l = idx < x.Length ? x.Array[idx] : 0;
                var r = idx < y.Length ? y.Array[idx] : 0;

                if (l == r) continue;

                return l.CompareTo(r);
            }

            return 0;
        }
    }
}