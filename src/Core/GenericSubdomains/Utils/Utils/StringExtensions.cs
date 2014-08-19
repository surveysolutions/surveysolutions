using System;

namespace WB.Core.GenericSubdomains.Utils
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string input)
        {
            if ((input == null || input.Length < 2))
                return input;

            var firstLetter = input.Substring(0, 1).ToLower();


            return firstLetter + input.Substring(1, input.Length - 1);
        }
    }
}