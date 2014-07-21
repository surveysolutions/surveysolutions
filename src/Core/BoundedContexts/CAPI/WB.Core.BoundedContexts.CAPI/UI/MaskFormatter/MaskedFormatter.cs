using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WB.Core.BoundedContexts.Capi.UI.MaskFormatter
{
    public class MaskedFormatter : IMaskedFormatter
    {
        private const char DigitKey = '9';
        private const char LiteralKey = '\'';
        private const char UppercaseKey = 'U';
        private const char LowercaseKey = 'L';
        private const char AlphaNumericKey = 'A';
        private const char CharacterKey = 'a';
        private const char AnythingKey = '*';
        private const char HexKey = 'H';

        private readonly String mask;
        private MaskCharacter[] maskChars;

        public MaskedFormatter(String mask, string validCharacters = null, string invalidCharacters = null, string placeholder = null, char placeholderCharacter = '_')
        {
            this.mask = mask;
            this.PlaceholderCharacter = placeholderCharacter;
            this.Placeholder = placeholder;
            this.InvalidCharacters = invalidCharacters??placeholderCharacter.ToString();
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

        public String ValueToString(string value, ref int oldCurstorPosition)
        {
            var stringValue = value ?? "";
            var result = new StringBuilder();

            int index = 0;
            int newCursorPosition = oldCurstorPosition;
            int lastSuccessfulIndex = 0;
            bool isLiteralAppearedAfterCursor = false;
            bool isAddedCharSuccessful = false;
            for (int i = 0; i < this.maskChars.Length; i++)
            {
                var oldIndex = index;
                var isCharAppended = this.maskChars[i].Append(result, stringValue, ref index, this.Placeholder);
                if (!isCharAppended)
                {
                    result.Append(this.maskChars[i][this.PlaceholderCharacter]);
                }
                else
                {
                    if (oldIndex == oldCurstorPosition - 1)
                    {
                        isAddedCharSuccessful = true;
                    }
                    if (oldIndex > oldCurstorPosition && !isLiteralAppearedAfterCursor)
                    {
                        if (maskChars[i].IsLiteral())
                            isLiteralAppearedAfterCursor = true;
                    }
                    if(!isLiteralAppearedAfterCursor)
                        lastSuccessfulIndex = i;
                }
            }

            if (oldCurstorPosition > this.maskChars.Length)
                newCursorPosition = this.maskChars.Length;

            if (value.Length > maskChars.Length || value.Length == 1)
            {
                if (!isAddedCharSuccessful)
                    newCursorPosition = oldCurstorPosition - 1;
                else
                    newCursorPosition = lastSuccessfulIndex + 1;
            }

            oldCurstorPosition = newCursorPosition;
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

        public string GetCleanText(string text)
        {
            int i = 0;
            return this.CleanUpLiterals(text, ref i);
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
                        case LiteralKey:
                        {
                            if (++counter < maxCounter)
                            {
                                maskChar = mask[counter];
                                temp.Add(new LiteralCharacter(this.InvalidCharacters, this.PlaceholderCharacter,
                                    this.ValidCharacters, maskChar));
                            }
                            break;
                        }
                        case UppercaseKey:
                        {
                            temp.Add(new UpperCaseCharacter(this.InvalidCharacters, this.PlaceholderCharacter,
                                this.ValidCharacters));
                            break;
                        }
                        case LowercaseKey:
                        {
                            temp.Add(new LowerCaseCharacter(this.InvalidCharacters, this.PlaceholderCharacter,
                                this.ValidCharacters));
                            break;
                        }
                        case AlphaNumericKey:
                        {
                            temp.Add(new AlphaNumericCharacter(this.InvalidCharacters, this.PlaceholderCharacter,
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
                        case HexKey:
                        {
                            temp.Add(new HexCharacter(this.InvalidCharacters, this.PlaceholderCharacter, this.ValidCharacters));
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

        private string CleanUpLiterals(String value, ref int oldCurstorPosition)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var result=new StringBuilder();
            int index = 0;
            bool isCursorPositionSet = false;
            for (int i = 0; i < this.maskChars.Length; i++)
            {
                if (index >= value.Length)
                    break;

                if (this.maskChars[i].IsLiteral())
                {
                    var character = value[index];
                    if (character == this.maskChars[i][character])
                    {
                        index++;
                    }
                }
                else
                {
                    while (!this.maskChars[i].IsValidCharacter(value[index]))
                    {
                        index++;
                        if (index >= value.Length)
                        {
                            if (!isCursorPositionSet)
                            {
                                oldCurstorPosition = result.Length;
                            }
                            return result.ToString();
                        }
                    }

                    result.Append(value[index]);


                    if (i + 1 == oldCurstorPosition && !isCursorPositionSet)
                    {
                        oldCurstorPosition = result.Length;
                        isCursorPositionSet = true;
                    }

                    index++;
                }
            }
            return result.ToString();
        }
    }
}