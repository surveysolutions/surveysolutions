using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WB.Core.SharedKernels.DataCollection.MaskFormatter
{
    public class MaskedFormatter
    {
        private const char DigitKey = '#';
        private const char CharacterKey = '~';
        private const char AnythingKey = '*';
        //private const char PlaceholderCharacter = '_';

        private string Mask { get; }
        private Regex regexpMask;

        public MaskedFormatter(String mask)
        {
            this.Mask = mask;
            this.UpdateInternalMask();
        }


        public bool IsTextMaskMatched(string text)
        {
            return regexpMask.IsMatch(text);
        }

        private void UpdateInternalMask()
        {
            StringBuilder sb = new StringBuilder('^');
            foreach (var maskChar in Mask)
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
                        sb.Append(maskChar);
                        break;
                }
            }
            sb.Append('$');

            this.regexpMask = new Regex(sb.ToString());
        }
    }
}