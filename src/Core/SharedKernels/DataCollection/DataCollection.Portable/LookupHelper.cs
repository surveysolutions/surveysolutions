using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection
{
    public static class LookupHelper
    {
        public static IEnumerable<string> SplitStringToLines(string value)
        {
            var subIndex = 0;
            var i = value.IndexOf('\n');

            while (i >= 0 && i < value.Length)
            {
                if (value[i] == '\n')
                {
                    string res;
                    if (i > 0 && value[i - 1] == '\r')
                    {
                        res = value.Substring(subIndex, i - subIndex - 1);
                    }
                    else
                    {
                        res = value.Substring(subIndex, i - subIndex);
                    }

                    if (!string.IsNullOrWhiteSpace(res))
                    {
                        yield return res.Trim();
                    }

                    subIndex = i + 1;
                }

                i = value.IndexOf('\n', i + 1);
            }

            var substring = value.Substring(subIndex);
            if (!string.IsNullOrWhiteSpace(substring))
            {
                yield return substring.Trim();
            }
        }
    }
}
