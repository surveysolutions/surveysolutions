using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.ExpressionStorage.CustomFunctions
{
    public static class Extensions
    {
        public static bool InRange(this int? value, int? low, int? high)
        {
            if (value < low) return false;
            if (value > high) return false;
            return true;
        }

        public static bool InList(this int? value, params int?[] valuesList) => 
            valuesList.Length != 0 && valuesList.Any(v => v == value);

        public static bool ContainsAll(this int[] multichoice, params int[] valuesList)
        {
            if (multichoice == null) return false;
            if (multichoice.Length == 0) return false;

            if (valuesList == null) return true;
            if (valuesList.Length == 0) return true;

            return valuesList.All(multichoice.Contains);
        }

        public static bool ContainsAny(this int[] multichoice, params int[] valuesList)
        {
            if (multichoice == null) return false;
            if (multichoice.Length == 0) return false;

            if (valuesList == null) return true;
            if (valuesList.Length == 0) return true;

            return valuesList.Any(value => multichoice.Any(multiEmelent => multiEmelent == value));
        }

        public static bool ContainsOnly(this int[] multichoice, params int[] valuesList) => 
            multichoice?.Length == valuesList.Length && valuesList.All(multichoice.Contains);
    }
}