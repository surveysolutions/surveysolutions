using System.Text;
using System.Text.RegularExpressions;

namespace WB.Core.SharedKernels.DataCollection.MaskFormatter
{
    public static class MaskedFormatter
    {
        private const char DigitKey = '#';
        private const char CharacterKey = '~';
        private const char AnythingKey = '*';

        public static bool IsTextMaskMatched(this string text, string mask)
        {
            var sb = new StringBuilder();
            foreach (var maskChar in mask)
            {
                switch (maskChar)
                {
                    case DigitKey:
                        sb.Append(@"\d");
                        break;
                    case CharacterKey:
                        sb.Append(@"\p{L}");
                        break;
                    case AnythingKey:
                        sb.Append('.');
                        break;
                    default:
                        sb.Append(Regex.Escape(maskChar.ToString()));
                        break;
                }
            }

            return Regex.IsMatch(text, $"^{sb}$");
        }
    }
}
