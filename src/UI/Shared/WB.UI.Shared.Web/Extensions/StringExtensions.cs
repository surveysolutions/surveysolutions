using System;
using System.IO;
using System.Linq;

namespace WB.UI.Shared.Web.Extensions
{
    public static class StringExtensions
    {
        public static string ToValidFileName(this string source)
        {
            return Path
                .GetInvalidFileNameChars()
                .Aggregate(
                    source.Substring(0, Math.Min(source.Length, 255)),
                    (current, c) => current.Replace(c, 'x'));
        }

        public static int ToInt(this string value, int @default = 0)
        {
            int result;

            return Int32.TryParse(value, out result) ? result : @default;
        }

        public static byte ToByte(this string value, byte @default = 0)
        {
            byte result;

            return Byte.TryParse(value, out result) ? result : @default;
        }

        public static bool ToBool(this string value, bool @default = false)
        {
            bool result;

            return value.HasValue() && Boolean.TryParse(value, out result) ? result : @default;
        }

        public static decimal ToDecimal(this string value, decimal @default = 0)
        {
            decimal result;

            return value.HasValue() && Decimal.TryParse(value, out result) ? result : @default;
        }

        public static DateTime ToDateTime(this string value, DateTime @default)
        {
            DateTime result;

            return value.HasValue() && DateTime.TryParse(value, out result) ? result : @default;
        }

        public static bool HasValue(this string value)
        {
            return !String.IsNullOrEmpty(value);
        }
    }
}
