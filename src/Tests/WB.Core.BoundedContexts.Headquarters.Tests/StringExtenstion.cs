using System;

namespace WB.Core.BoundedContexts.Headquarters.Tests
{
    internal static class StringExtenstion
    {
        public static string[] ToSeparateWords(this string text)
        {
            return text.Split(new[] { ' ', ',', '.', ':', ';', '-' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}