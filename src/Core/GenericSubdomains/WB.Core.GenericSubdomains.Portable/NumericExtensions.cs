﻿using System;
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
            if (totalCount == 0) return 0;
            if (source > totalCount) return 100;
            return (decimal)source / totalCount * 100;
        }

        public static string FormatDecimal(this decimal source, int precision = 16)
            => ((decimal?)source).FormatDecimal(precision);

        public static string FormatDecimal(this decimal? source, int precision = 16)
        {
            if (!source.HasValue) return string.Empty;

            return FormatNumber(format => source.Value.ToString(format, CultureInfo.CurrentCulture), precision);
        }
        
        public static string FormatDouble(this double source, int precision = 16)
            => ((double?)source).FormatDouble(precision);

        public static string FormatDouble(this double? source, int precision = 2)
        {
            if (!source.HasValue) return string.Empty;

            return FormatNumber(format => source.Value.ToString(format, CultureInfo.CurrentCulture), precision);
        }
        
        public static string FormatInt(this int source)
            => ((int?)source).FormatInt();

        public static string FormatInt(this int? source)
        {
            if (!source.HasValue) return string.Empty;

            return FormatNumber(format => source.Value.ToString(format, CultureInfo.CurrentCulture), 0);
        }


        private static string FormatNumber(Func<string, string> formatNumber, int precision)
        {
            var mantissaFormat = new string('#', precision);
            var groupSeparator = CultureInfo.InvariantCulture.NumberFormat.CurrencyGroupSeparator;
            var decimalSeparator = CultureInfo.InvariantCulture.NumberFormat.CurrencyDecimalSeparator;
            string format = $"#{groupSeparator}0{decimalSeparator}{mantissaFormat}";

            var valueAsString = formatNumber.Invoke(format);

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
