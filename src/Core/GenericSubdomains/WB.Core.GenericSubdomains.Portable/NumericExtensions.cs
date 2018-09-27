using System;
using System.Globalization;
using System.Linq;

namespace WB.Core.GenericSubdomains.Portable
{
    public static class NumericExtensions
    {
        public static int PercentOf(this long source, long totalCount)
        {
            if (source > totalCount) return 100;
            return (int)((decimal)source / totalCount * 100);
        }

        public static decimal ToPercentOf(this long source, long totalCount)
        {
            if (source > totalCount) return 100;
            return (decimal)source / totalCount * 100;
        }

        public static string FormatDecimal(this decimal source, int precigion = 16)
            => ((decimal?)source).FormatDecimal(precigion);

        public static string FormatDecimal(this decimal? source, int precigion = 16)
        {
            if (!source.HasValue) return string.Empty;

            var mantissaFormat = new string('#', precigion);
            var groupSeparator = CultureInfo.InvariantCulture.NumberFormat.CurrencyGroupSeparator;
            var decimalSeparator = CultureInfo.InvariantCulture.NumberFormat.CurrencyDecimalSeparator;
            string format = $"#{groupSeparator}0{decimalSeparator}{mantissaFormat}";

            var valueAsString = source.Value.ToString(format, CultureInfo.CurrentCulture);

            valueAsString = FixLeadingZeroes(valueAsString);

            return valueAsString;
        }

        private static string FixLeadingZeroes(string valueAsString)
        {
            var currencyDecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator;
            var negativeInfinitySymbol = CultureInfo.CurrentCulture.NumberFormat.NegativeSign;
            if (valueAsString.StartsWith(currencyDecimalSeparator))
            {
                valueAsString = "0" + valueAsString;
            }
            var negativeTextWithDecimalSeparator = negativeInfinitySymbol +
                                                   currencyDecimalSeparator;
            if (valueAsString.StartsWith(negativeTextWithDecimalSeparator))
            {
                var stringWithoutNegativeSignAndDot = valueAsString.Replace(negativeTextWithDecimalSeparator, string.Empty);

                valueAsString =
                    $"{negativeInfinitySymbol}0{currencyDecimalSeparator}{stringWithoutNegativeSignAndDot}";
            }
            return valueAsString;
        }

        public static int[] ToIntArray(this decimal[] value) => value?.Select(x => (int) x)?.ToArray();
    }
}
