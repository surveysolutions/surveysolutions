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
            var stringValue = value ?? "";
            var result = new StringBuilder();

            int index = 0;
            int newCursorPosition = oldCursorPosition;
            int lastSuccessfulIndex = 0;
            bool isLiteralAppearedAfterCursor = false;
            bool isAddedLastCharSuccessful = false;
            for (int i = 0; i < this.maskChars.Length; i++)
            {
                var oldIndex = index;
                if (stringValue.Length > maskChars.Length && oldCursorPosition == index)
                {
                    index++;
                }

                if (!this.maskChars[i].Append(result, stringValue, ref index, this.Placeholder))
                {
                    result.Append(this.maskChars[i][this.PlaceholderCharacter]);
                }
                else
                {
                    if (oldIndex == oldCursorPosition - 1)
                    {
                        isAddedLastCharSuccessful = true;
                    }
                    if (oldIndex > oldCursorPosition && !isLiteralAppearedAfterCursor)
                    {
                        if (maskChars[i].IsLiteral())
                            isLiteralAppearedAfterCursor = true;
                    }
                    if (!isLiteralAppearedAfterCursor)
                        lastSuccessfulIndex = i;
                }
            }

            if (oldCursorPosition > this.maskChars.Length)
                newCursorPosition = this.maskChars.Length;

            if (stringValue.Length > maskChars.Length || stringValue.Length == 1)
            {
                if (isAddedLastCharSuccessful)
                    newCursorPosition = lastSuccessfulIndex + 1;
                else
                    newCursorPosition = oldCursorPosition - 1;
            }

            oldCursorPosition = newCursorPosition;
            return result.ToString();
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

            String mask = this.Mask;
            if (mask != null)
            {
                for (int counter = 0, maxCounter = mask.Length; counter < maxCounter; counter++)
                {
                    char maskChar = mask[counter];

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
                            temp.Add(new MaskCharacter(this.InvalidCharacters, this.PlaceholderCharacter,
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