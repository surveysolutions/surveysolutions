using System;
using System.Collections.Generic;
using System.Text;

namespace WB.UI.Shared.Android.Controls.MaskedEditTextControl
{
    public class MaskedFormatter
    {

        private const char DigitKey = '9';
        private const char LiteralKey = '\'';
        private const char UppercaseKey = 'U';
        private const char LowercaseKey = 'L';
        private const char AlphaNumericKey = 'a';
        private const char CharacterKey = '?';
        private const char AnythingKey = '*';
        private const char HexKey = 'H';

        private readonly MaskCharacter[] emptyMaskChars = new MaskCharacter[0];

        private String mMask;
        private MaskCharacter[] mMaskChars;

        public MaskedFormatter()
        {
            mMaskChars = this.emptyMaskChars;
            this.PlaceholderCharacter = '_';
        }

        public MaskedFormatter(String mask)
            : this()
        {
            Mask = mask;
        }

        public string Mask
        {
            get { return this.mMask; }
            set
            {
                this.mMask = value;
                this.UpdateInternalMask();
            }
        }

        public string ValidCharacters { get; set; }

        public string InvalidCharacters { get; set; }

        public string Placeholder { get; set; }

        public char PlaceholderCharacter { get; set; }

        public String ValueToString(Object value, ref int oldCurstorPosition)
        {
            var stringValue = (value == null) ? "" : value.ToString();

            if (string.IsNullOrEmpty(Mask))
            {
                return stringValue;
            }

            var result = new StringBuilder();

            int index = 0;

            bool newCursorPositionReceived = false;

            stringValue = this.CleanUpLiterals(stringValue, this.mMaskChars, ref oldCurstorPosition);

            for (int i = 0; i < this.mMaskChars.Length; i++)
            {
                if (index == oldCurstorPosition && !newCursorPositionReceived)
                {
                    oldCurstorPosition = i;
                    newCursorPositionReceived = true;
                }

                if (!this.mMaskChars[i].Append(result, stringValue, ref index, this.Placeholder))
                {
                    result.Append(this.mMaskChars[i][this.PlaceholderCharacter]);
                }
            }

            return result.ToString();
        }

        public bool IsTextMaskMatched(string text)
        {
            if (string.IsNullOrEmpty(Mask))
            {
                return true;
            }

            String stringValue = text ?? "";
            var result = new StringBuilder();
            int index = 0;
            for (int i = 0; i < mMaskChars.Length; i++)
            {
                if (!mMaskChars[i].Append(result, stringValue, ref index, Placeholder))
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

            String mask = Mask;
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
                mMaskChars = this.emptyMaskChars;
            }
            else
            {
                mMaskChars = fixedCharacters.ToArray();
            }
        }

        private string CleanUpLiterals(String value, MaskCharacter[] mask,
            ref int oldCurstorPosition)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var result=new StringBuilder();
            bool newCursorPositionReceived = false;
            int index = 0;
            for (int i = 0; i < mask.Length; i++)
            {
                if (index >= value.Length)
                    break;
                
                if (index == oldCurstorPosition && !newCursorPositionReceived)
                {
                    oldCurstorPosition = result.Length;
                    newCursorPositionReceived = true;
                }

                if (mask[i].IsLiteral())
                {
                    var character = value[index];
                    if (character == mask[i][character])
                    {
                        index++;
                    }
                }
                else
                {
                    while (!mask[i].IsValidCharacter(value[index]))
                    {
                        index++;
                        if (index >= value.Length)
                            return result.ToString();
                    }
                    result.Append(value[index]);
                    index++;
                }
            }
            return result.ToString();
        }
    }
}