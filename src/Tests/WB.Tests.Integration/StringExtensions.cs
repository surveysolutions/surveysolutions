using System;

namespace WB.Tests.Integration
{
    internal static class StringExtensions
    {
        public static string[] ToSeparateWords(this string sentence)
        {
            return sentence.Split(new[] { ' ', ',', '.', ';', ':', '-' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}