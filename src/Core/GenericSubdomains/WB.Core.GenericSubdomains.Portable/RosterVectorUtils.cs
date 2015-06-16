using System;
using System.Linq;

namespace WB.Core.GenericSubdomains.Portable
{
    public static class RosterVectorUtils
    {
        public static bool Identical(this decimal[] vector1, decimal[] vector2)
        {
            if (vector1 == null) throw new ArgumentNullException("vector1");
            if (vector2 == null) return false;

            if (vector1.Length == 0 && vector2.Length == 0)
            {
                return true;
            }

            if (vector1.Length != vector2.Length)
            {
                return false;
            }

            return vector1.SequenceEqual(vector2);
        }
    }
}