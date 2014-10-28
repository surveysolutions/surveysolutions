using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WB.Core.BoundedContexts.Capi.UI.MaskFormatter
{
    public class MaskedFormatter : IMaskedFormatter
    {
        private const char DigitKey = '#';
        private const char CharacterKey = '~';
        private const char AnythingKey = '*';
       /* private const char LiteralKey = '\'';
        private const char UppercaseKey = 'U';
        private const char LowercaseKey = 'L';
        private const char AlphaNumericKey = 'A';
        private const char HexKey = 'H';*/

        private readonly String mask;
        internal MaskCharacter[] maskChars;

        public MaskedFormatter(String mask, string validCharacters = null, string invalidCharacters = null, string placeholder = null, char placeholderCharacter = '_')
        {
            this.mask = mask;
            this.PlaceholderCharacter = placeholderCharacter;
            this.Placeholder = placeholder;
            this.InvalidCharacters = invalidCharacters ?? placeholderCharacter.ToString();
            this.ValidCharacters = validCharacters;
            this.UpdateInternalMask();
        }

        public string Mask
        {
            get { return this.mask; }
        }

        public string ValidCharacters { get;private set; }

        public string InvalidCharacters { get; private set; }

        public string Placeholder { get; private set; }

        public char PlaceholderCharacter { get; private set; }

        public string FormatValue(string value, ref int oldCursorPosition)
        {
            bool isIncreasing = IsIncreasedValue(value);

            if (isIncreasing)
            {
                oldCursorPosition = FixCursorPosition(value, oldCursorPosition);
            }

            value = AddMaskedCharacters(value, isIncreasing, oldCursorPosition);

            var result = new StringBuilder();

            int index = 0;

            for (int i = 0; i < this.maskChars.Length; i++)
            {
                if (!this.maskChars[i].Append(result, value, ref index, this.Placeholder))
                {
                    result.Append(this.maskChars[i][this.PlaceholderCharacter]);
                }
            }

            oldCursorPosition = GetNewCursorPosition(value, isIncreasing, oldCursorPosition);

            return result.ToString();
        }

        private int FixCursorPosition(string value, int oldCursorPosition)
        {
            if (value.Length > 0 && oldCursorPosition == 0)
                return value.Length;

            return oldCursorPosition;
        }

        private bool IsIncreasedValue(string value)
        {
            return value.Length == 1 || value.Length > this.maskChars.Length;
        }

        private int GetNewCursorPosition(string value, bool isIncreasing, int oldCursorPosition)
        {
            if (oldCursorPosition > this.maskChars.Length)
                return this.maskChars.Length;

            if (!isIncreasing)
                return oldCursorPosition;
        
            var index = oldCursorPosition - 1;

            var result = new StringBuilder(value.Substring(0, index));

            for (var i = index; i < this.maskChars.Length; i++)
            {
                if (!this.maskChars[i].IsLiteral() && index > oldCursorPosition - 1)
                    return i;

                if (!this.maskChars[i].Append(result, value, ref index, this.Placeholder))
                {
                    return i;
                }
            }

            return Math.Min(oldCursorPosition + 1, value.Length);
        }

        private string AddMaskedCharacters(string value, bool isIncreasing, int oldCursorPosition)
        {
            if (isIncreasing)
            {
                if (value.Length > oldCursorPosition)
                {
                    return value.Remove(oldCursorPosition, value.Length - this.maskChars.Length);
                }
            }
            else
            {
                return value.Insert(oldCursorPosition,
                    new string(
                        Enumerable.Range(0, this.maskChars.Length - value.Length)
                            .Select(i => this.maskChars[i + oldCursorPosition][this.PlaceholderCharacter])
                            .ToArray()));

            }
            return value;
        }

        public bool IsTextMaskMatched(string text)
        {
            String stringValue = text ?? "";
            var result = new StringBuilder();
            int index = 0;
            for (int i = 0; i < this.maskChars.Length; i++)
            {
                if (!this.maskChars[i].Append(result, stringValue, ref index, this.Placeholder))
                {
                    return false;
                }
            }
            return true;
        }

        private void UpdateInternalMask()
        {
            var fixedCharacters = new List<MaskCharacter>();
            var temp = fixedCharacters;

            var tempMask = this.Mask;
            if (tempMask != null)
            {
                for (int counter = 0, maxCounter = tempMask.Length; counter < maxCounter; counter++)
                {
                    char maskChar = tempMask[counter];

                    switch (maskChar)
                    {
                        case DigitKey:
                        {
                            temp.Add(new DigitMaskCharacter(this.InvalidCharacters, this.PlaceholderCharacter,
                                this.ValidCharacters));
                            break;
                        }
                        case CharacterKey:
                        {
                            temp.Add(new CharCharacter(this.InvalidCharacters, this.PlaceholderCharacter,
                                this.ValidCharacters));
                            break;
                        }
                        case AnythingKey:
                        {
                            temp.Add(new MaskCharacter(InvalidCharacters, this.PlaceholderCharacter,
                                this.ValidCharacters));
                            break;
                        }
                        default:
                        {
                            temp.Add(new LiteralCharacter(this.InvalidCharacters, this.PlaceholderCharacter,
                                this.ValidCharacters, maskChar));
                            break;
                        }
                    }
                }
            }
            if (fixedCharacters.Count == 0)
            {
                throw new ArgumentException("mask is empty");
            }
            else
            {
                this.maskChars = fixedCharacters.ToArray();
            }
        }
    }
}