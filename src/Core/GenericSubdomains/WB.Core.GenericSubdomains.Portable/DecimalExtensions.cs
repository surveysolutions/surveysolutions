using System;
using System.Globalization;
using System.Text.RegularExpressions;

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
            string format = $"#{groupSeparator}#{decimalSeparator}{mantissaFormat}";

            var valueAsString = source.Value.ToString(format, CultureInfo.CurrentCulture);

            return valueAsString;
        }
    }
}