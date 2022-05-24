using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace WB.Services.Infrastructure
{
    public static class CommonExtensions
    {
        public static string ToSnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input)) { return input; }

            var startUnderscores = Regex.Match(input, @"^_+");
            return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
        }

        [DebuggerStepThrough]
        public static string FormatGuid(this Guid guid)
        {
            return guid.ToString("N");
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> items) where TKey 
            : notnull => items.ToDictionary(item => item.Key, item => item.Value);
    }
}
