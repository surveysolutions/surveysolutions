using System;

namespace WB.Core.SharedKernels.DataCollection.Tests
{
    internal static class StringExtensions
    {
        public static string[] ToSeparateWords(this string sentence)
        {
            return sentence.Split(new[] { ' ', ',', '.', ';', ':', '-' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}