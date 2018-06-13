// ReSharper disable once CheckNamespace
namespace WB.Core.SharedKernels.DataCollection.ExpressionStorage.CustomFunctions
{
    // backward compatibility - KP-11462
    public static class Extensions
    {
        public static bool InRange(int value, int low, int high)
        {
            return value.InRange(low, high);
        }

        public static bool InRange(int? value, int? low, int? high)
        {
            return value.InRange(low, high);
        }

        public static bool InRange(int? value, double? low, double? high)
        {
            return value.InRange(low, high);
        }

        public static bool InList(int? value, params int?[] valuesList)
        {
            return value.InList(valuesList);
        }

        public static bool InList(int value, params int?[] valuesList) =>
            value.InList(valuesList);

        public static bool ContainsAll(int[] multichoice, params int[] valuesList)
        {
            return multichoice.ContainsAll(valuesList);
        }

        public static bool ContainsAll(int[] multichoice, params decimal[] valuesList)
        {
            return multichoice.ContainsAll(valuesList);
        }

        public static bool ContainsAny(int[] multichoice, params int[] valuesList)
        {
            return multichoice.ContainsAny(valuesList);
        }

        public static bool ContainsOnly(int[] multichoice, params int[] valuesList) =>
            multichoice.ContainsOnly(valuesList);
    }
}
