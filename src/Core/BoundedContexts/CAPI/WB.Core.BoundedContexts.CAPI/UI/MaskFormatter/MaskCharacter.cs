using System;
using System.Text;

namespace WB.Core.BoundedContexts.Capi.UI.MaskFormatter
{
    internal class MaskCharacter
    {
        public MaskCharacter(string invalidCharacters, char placeholderCharacter, string validCharacters)
        {
            this.ValidCharacters = validCharacters;
            this.PlaceholderCharacter = placeholderCharacter;
            this.InvalidCharacters = invalidCharacters;
        }

        public virtual bool IsLiteral()
        {
            return false;
        }

        public virtual bool IsValidCharacter(char character)
        {
            if (this.IsLiteral())
            {
                return this[character] == character;
            }

            character = this[character];

            String filter = this.ValidCharacters;
            if (filter != null && filter.IndexOf(character) == -1)
            {
                return false;
            }

            filter = this.InvalidCharacters;
            if (filter != null && filter.IndexOf(character) != -1)
            {
                return false;
            }

            return true;
        }

        public string InvalidCharacters { get;private set; }
        public char PlaceholderCharacter { get; private set; }
        public string ValidCharacters { get;private set; }

        public virtual char this[char character]
        {
            get { return character; }
        }

        public bool Append(StringBuilder buffer, String formatting, ref int index, String placeholder)
        {
            var inString = index < formatting.Length;
            char character = inString ? formatting[index] : '0';

            if (!inString)
            {
                return false;
            }

            if (this.IsLiteral())
            {
                buffer.Append(this[character]);

                if (character == this[character])
                {
                    index = index + 1;
                }
              
            }
            else if (index >= formatting.Length)
            {
                if (placeholder != null && index < placeholder.Length)
                {
                    buffer.Append(placeholder[index]);
                }
                else
                {
                    buffer.Append(this.PlaceholderCharacter);
                }

                index = index + 1;
            }
            else if (this.IsValidCharacter(character))
            {
                buffer.Append(this[character]);
                index = buffer.Length;
            }
            else
            {
                index = index + 1;
                return false;
            }

            return true;
        }

    }

    internal class LiteralCharacter : MaskCharacter
    {
        private readonly char mLiteralCharacter;

        public LiteralCharacter(string invalidCharacters, char placeholderCharacter, string validCharacters, char mLiteralCharacter)
            : base(invalidCharacters, placeholderCharacter, validCharacters)
        {
            this.mLiteralCharacter = mLiteralCharacter;
        }

        public override bool IsLiteral()
        {
            return true;
        }

        public override char this[char character]
        {
            get { return this.mLiteralCharacter; }
        }
    }

    internal class DigitMaskCharacter : MaskCharacter
    {
        public DigitMaskCharacter(string invalidCharacters, char placeholderCharacter, string validCharacters)
            : base(invalidCharacters, placeholderCharacter, validCharacters) {}

        public override bool IsValidCharacter(char character)
        {
            return Char.IsDigit(character) && base.IsValidCharacter(character);
        }
    }

    internal class UpperCaseCharacter : MaskCharacter
    {
        public UpperCaseCharacter(string invalidCharacters, char placeholderCharacter, string validCharacters)
            : base(invalidCharacters, placeholderCharacter, validCharacters) {}

        public override bool IsValidCharacter(char character)
        {
            return Char.IsLetter(character) && base.IsValidCharacter(character);
        }

        public override char this[char character]
        {
            get { return Char.ToUpper(character); }
        }
    }

    internal class LowerCaseCharacter : MaskCharacter
    {
        public LowerCaseCharacter(string invalidCharacters, char placeholderCharacter, string validCharacters)
            : base(invalidCharacters, placeholderCharacter, validCharacters) {}

        public override bool IsValidCharacter(char character)
        {
            return Char.IsLetter(character) && base.IsValidCharacter(character);
        }
        public override char this[char character]
        {
            get { return Char.ToLower(character); }
        }
    }

    internal class AlphaNumericCharacter : MaskCharacter
    {
        public AlphaNumericCharacter(string invalidCharacters, char placeholderCharacter, string validCharacters)
            : base(invalidCharacters, placeholderCharacter, validCharacters) {}

        public override bool IsValidCharacter(char character)
        {
            return Char.IsLetterOrDigit(character) && base.IsValidCharacter(character);
        }
    }

    internal class CharCharacter : MaskCharacter
    {
        public CharCharacter(string invalidCharacters, char placeholderCharacter, string validCharacters)
            : base(invalidCharacters, placeholderCharacter, validCharacters) {}

        public override bool IsValidCharacter(char character)
        {
            return Char.IsLetter(character) && base.IsValidCharacter(character);
        }
    }

    internal class HexCharacter : MaskCharacter
    {
        private const String HexChars = "0123456789abcedfABCDEF";

        public HexCharacter(string invalidCharacters, char placeholderCharacter, string validCharacters)
            : base(invalidCharacters, placeholderCharacter, validCharacters) {}

        public override bool IsValidCharacter(char character)
        {
            return HexChars.IndexOf(character) != -1 && base.IsValidCharacter(character);
        }

        public override char this[char character]
        {
            get
            {
                if (Char.IsDigit(character))
                {
                    return character;
                }

                return Char.ToUpper(character);
            }
        }
    }
}