using System;
using System.Linq;

namespace WB.Core.GenericSubdomains.Portable
{
    public static class RosterVectorUtils
    {
        public static bool Identical(this decimal[] vector1, decimal[] roster2)
        {
            if (vector1 == null) throw new ArgumentNullException("vector1");
            if (roster2 == null) return false;

            if (vector1.Length == 0 && roster2.Length == 0)
            {
                return true;
            }

            return vector1.SequenceEqual(roster2);
        }
    }
}