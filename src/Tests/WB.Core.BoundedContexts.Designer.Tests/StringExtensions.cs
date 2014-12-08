using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Tests.Unit
{
    internal static class StringExtensions
    {
        public static string[] ToSeparateWords(this string sentence)
        {
            return sentence.Split(new[] { ' ', ',', '.', ';', ':', '-' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}