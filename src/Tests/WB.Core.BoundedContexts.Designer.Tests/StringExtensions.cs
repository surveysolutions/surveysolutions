using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.Tests
{
    internal static class StringExtensions
    {
        public static string[] ToSeparateWords(this string sentence)
        {
            return sentence.Split(new[] { ' ', ',', '.', ';', ':', '-' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}