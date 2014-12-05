﻿using System;
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

        public static int ToInt(this string value, int @default)
        {
            int result;

            return Int32.TryParse(value, out result) ? result : @default;
        }

        public static bool IsDecimal(this string value)
        {
            decimal result;

            return Decimal.TryParse(value, out result);
        }

        public static bool ToBool(this string value, bool @default)
        {
            bool result;

            return Boolean.TryParse(value, out result) ? result : @default;
        }
    }
}
