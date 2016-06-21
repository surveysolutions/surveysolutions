using System;
using System.Globalization;

namespace WB.Core.GenericSubdomains.Portable
{
    public static class DecimalExtensions
    {
        public static string FormatDecimal(this decimal source, int presigion = 16)
        {
            return FormatDecimal((decimal?) source, presigion);
        }

        public static string FormatDecimal(this decimal? source, int presigion = 16)
        {
            if (!source.HasValue) return String.Empty;

            var mantissaFormat = new string('#', presigion);
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
    }
}