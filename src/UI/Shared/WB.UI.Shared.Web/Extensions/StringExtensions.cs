﻿using System;
using System.IO;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

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

        public static int ToIntOrDefault(this string value, int @default) => value.ParseIntOrNull() ?? @default;

        public static bool IsDecimal(this string value)
        {
            decimal result;

            return decimal.TryParse(value, out result);
        }

        public static string Ellipsis(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;

            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }
    }
}
