using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WB.Core.BoundedContexts.Headquarters
{
    public static class Utils
    {
        private static readonly Regex RemoveNewLineRegEx = new Regex(@"\t|\n|\r", RegexOptions.Compiled);
        public static string RemoveNewLine(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : RemoveNewLineRegEx.Replace(value, " ");
        }

        public static int[] ParseMinusDelimitedIntArray(this string arrayString)
        {
            if (string.IsNullOrWhiteSpace(arrayString) || string.IsNullOrWhiteSpace(arrayString.Trim('_'))) return null;

            //"-1-2--3".Split('-') => string[5] { "", "1", "2", "", "3" }
            // every empty space mean that we encounter negative number
            var items = arrayString.Split('-');
            var result = new List<int>();

            for (int i = 0; i < items.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(items[i]))
                {
                    if (i == items.Length - 1) // if this is last item in array
                        continue;

                    // parse next item and increment index
                    result.Add(-int.Parse(items[++i]));
                }
                else
                    result.Add(int.Parse(items[i]));
            }

            return result.ToArray();
        }
    }
}
