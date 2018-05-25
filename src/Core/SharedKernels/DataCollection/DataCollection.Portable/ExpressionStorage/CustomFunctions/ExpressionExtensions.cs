using System;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace WB.Core.SharedKernels.DataCollection.ExpressionStorage.CustomFunctions
{
    public static class ExpressionExtensions
    {
        public static bool InRange(this int value, int low, int high)
        {
            if (value < low) return false;
            if (value > high) return false;
            return true;
        }

        public static bool InRange(this int? value, int? low, int? high)
        {
			if (value == null || low == null || high == null) return false;
            if (value < low) return false;
            if (value > high) return false;
            return true;
        }

        // backward compatibility
        public static bool InRange(this int? value, double? low, double? high)
        {
			if (value == null || low == null || high == null) return false;
            if (value < low) return false;
            if (value > high) return false;
            return true;
        }

        public static bool InList(this int? value, params int?[] valuesList)
        {
            if (valuesList.Length == 0) return false;

            for (var index = 0; index < valuesList.Length; index++)
            {
                var v = valuesList[index];
                if (v == value) return true;
            }

            return false;
        }

        public static bool InList(this int value, params int?[] valuesList) =>
            ((int?)value).InList(valuesList);

        public static bool ContainsAll(this int[] multichoice, params int[] valuesList)
        {
            if (multichoice == null) return false;
            if (multichoice.Length == 0) return false;

            if (valuesList == null) return true;
            if (valuesList.Length == 0) return true;

            return valuesList.All(multichoice.Contains);
        }

        // backward compatibility
        public static bool ContainsAll(this int[] multichoice, params decimal[] valuesList)
        {
            return ContainsAll(multichoice, valuesList?.Select(Convert.ToInt32).ToArray() ?? new int[0]);
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

        /// <summary>
        /// For a single choice question checks that the selection is not mentioned in the specified list of values.
        /// </summary>
        /// <param name="singlechoice">single choice variable.</param>
        /// <param name="valuesList">list of values (blacklist).</param>
        /// <returns>True if the selection is not mentioned in the specified list of values.</returns>
        public static bool IsNoneOf(this int? singlechoice, params int[] valuesList)
        {
            if (valuesList == null) return true;
            if (valuesList.Length == 0) return true;
            if (singlechoice.HasValue == false) return true;
            return !valuesList.ContainsAny(singlechoice.Value);
        }

        #region March 2016 functions

        public static bool ContainsAnyOtherThan(this int[] multichoice, params int[] valuesList)
        {
            if (multichoice == null) return false;
            if (multichoice.Length == 0) return false;

            if (valuesList == null) return true;
            if (valuesList.Length == 0) return true;

            for (var j = 0; j < multichoice.Length; j++)
            {
                var other = true;
                for (var i = 0; i < valuesList.Length; i++)
                    if (multichoice[j] == valuesList[i]) other = false;
                if (other) return true;
            }

            return false;
        }

        #endregion
    }
}
