using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Android.Content;
using Android.Content.Res;
using Android.Text;
using Android.Text.Method;
using Android.Util;
using Android.Widget;
using Java.Lang;
using WB.Core.GenericSubdomains.Portable;
using String = System.String;

namespace WB.UI.Shared.Enumerator.CustomControls
{
    public class NumericValueChangedEventArgs : EventArgs
    {
        public decimal? NewValue { get; set; }

        public NumericValueChangedEventArgs(decimal? newValue)
        {
            this.NewValue = newValue;
        }
    }

    /// <summary>
    /// Numeric Edit Text that supports decimal separator and group separator according to your culture.
    /// </summary>
    public class NumericEditText : EditText
    {
        private class CustomKeyListener : DigitsKeyListener
        {
            private readonly string groupingSeparator = CultureInfo.CurrentCulture.NumberFormat.CurrencyGroupSeparator;
            private readonly string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            private readonly string negativeSign = CultureInfo.CurrentCulture.NumberFormat.NegativeSign;

            private readonly bool isDecimal;
            private readonly int maxDigitsBeforeDecimal;
            private readonly int maxDigitsAfterDecimal;

            public CustomKeyListener(bool isDecimal, int maxDigitsBeforeDecimal, int maxDigitsAfterDecimal) : base(true, isDecimal)
            {
                this.isDecimal = isDecimal;
                this.maxDigitsBeforeDecimal = maxDigitsBeforeDecimal;
                this.maxDigitsAfterDecimal = maxDigitsAfterDecimal;
            }

            public override ICharSequence FilterFormatted(ICharSequence source, int start, int end, ISpanned dest, int dstart, int dend)
            {
                var allowedSymbols = $"0123456789{this.negativeSign}";
                if (this.isDecimal)
                {
                    allowedSymbols += $"{nonLocalizedAndroidDecimalSeparator}{this.decimalSeparator}";
                }

                var hasNonLocalizedAndroidDecimalSeparator = source.ToString() == nonLocalizedAndroidDecimalSeparator && this.decimalSeparator != nonLocalizedAndroidDecimalSeparator;
                var hasDecimalSeperatorInInteger = (source.ToString() == this.decimalSeparator || hasNonLocalizedAndroidDecimalSeparator) && !this.isDecimal;

                if (hasDecimalSeperatorInInteger || !source.ToString().All(x => allowedSymbols.Contains(x)))
                {
                    return new Java.Lang.String("");
                }

                if(hasNonLocalizedAndroidDecimalSeparator) source = new Java.Lang.String(this.decimalSeparator);

                var enteredText = dest.ToString().Insert(dstart, source.ToString());

                var hasTextNegativeSign = enteredText.StartsWith(this.negativeSign);
                string textWithoutSign = hasTextNegativeSign ? enteredText.Length == 1 ? "" : enteredText.Substring(1, enteredText.Length - 1) : enteredText;

                string[] integerAndFraction = textWithoutSign.Split(this.decimalSeparator.ToCharArray());
                string integer = (integerAndFraction[0] ?? "").Replace(this.groupingSeparator, "");
                string fraction = integerAndFraction.Length > 1 ? integerAndFraction[1] ?? "" : "";

                var varifiers = new Func<bool>[]
                {
                () =>
                {
                    var countOfNegativeSigns = this.CountMatches(enteredText, this.negativeSign);
                    return countOfNegativeSigns > 1 || (countOfNegativeSigns == 1 && !hasTextNegativeSign);
                },
                () => enteredText.StartsWith(this.decimalSeparator) || enteredText.StartsWith(nonLocalizedAndroidDecimalSeparator),
                () =>
                {
                    int decimalPointPosition = enteredText.IndexOf(this.decimalSeparator);
                    return decimalPointPosition > 0 && textWithoutSign.Substring(decimalPointPosition).IndexOf(this.groupingSeparator) > 0;
                },
                () => textWithoutSign.Length == 1 && textWithoutSign == this.decimalSeparator,
                () => this.CountMatches(enteredText, this.decimalSeparator) > 1,
                () => fraction.Length > maxFractionDigits,
                () => (integer.Length + fraction.Length) > maxDigitsInDecimal,
                () => this.maxDigitsBeforeDecimal > 0 && integer.Length > this.maxDigitsBeforeDecimal,
                () => this.maxDigitsAfterDecimal > 0 && fraction.Length > this.maxDigitsAfterDecimal,
                () =>
                {
                    if (textWithoutSign.Length <= 2) return false;

                    string lastChar = textWithoutSign[textWithoutSign.Length - 1].ToString();
                    string secToLastChar = textWithoutSign[textWithoutSign.Length - 2].ToString();
                    if (lastChar != this.decimalSeparator && lastChar != this.groupingSeparator) return false;

                    return lastChar == secToLastChar;
                }
                };

                return varifiers.Any(isInvalid => isInvalid())
                    ? new Java.Lang.String("")
                    : hasNonLocalizedAndroidDecimalSeparator ? new Java.Lang.String(this.decimalSeparator) : null;
            }



            private int CountMatches(string str, string sub)
            {
                if (TextUtils.IsEmpty(str))
                {
                    return 0;
                }
                int lastIndex = str.LastIndexOf(sub);
                if (lastIndex < 0)
                {
                    return 0;
                }
                else
                {
                    return 1 + this.CountMatches(str.Substring(0, lastIndex), sub);
                }
            }
        }

        private const int maxDigitsInDecimal = 16;
        private const int maxFractionDigits = 15;
        private const string leadingZeroFilterRegex = "^0+(?!$)";
        private const string nonLocalizedAndroidDecimalSeparator = ".";

        private readonly string numberFilterRegex = "[^\\d\\" + CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator + "]";
        private readonly string groupingSeparator = CultureInfo.CurrentCulture.NumberFormat.CurrencyGroupSeparator;
        private readonly string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        private readonly string negativeSign = CultureInfo.CurrentCulture.NumberFormat.NegativeSign;

        private int maxDigitsBeforeDecimal;
        /// <summary>
        /// Gets or sets the maximum number of digits before the decimal point.
        /// </summary>
        /// <value>The maximum number of digits before the decimal point</value>
        public int MaxDigitsBeforeDecimal
        {
            get { return this.maxDigitsBeforeDecimal; }
            set
            {
                if (value + this.maxDigitsAfterDecimal > maxDigitsInDecimal)
                {
                    throw new ArgumentOutOfRangeException(nameof(MaxDigitsAfterDecimal),
                        $"Sum of digits before and after decimal seperator should be less than {maxDigitsInDecimal + 1}");
                }

                this.maxDigitsBeforeDecimal = value;
                this.InitKeyboard();
            }
        }

        private int maxDigitsAfterDecimal;

        /// <summary>
        /// Gets or sets the maximum number of digits after the decimal point.
        /// </summary>
        /// <value>The maximum number of digits after the decimal point</value>
        public int MaxDigitsAfterDecimal
        {
            get
            {
                return this.maxDigitsAfterDecimal;
            }
            set
            {
                if (value > maxFractionDigits)
                {
                    throw new ArgumentOutOfRangeException(nameof(MaxDigitsAfterDecimal),
                        $"Number of digits after decimal seperator should be less than {maxFractionDigits + 1}");
                }
                if (value + this.maxDigitsBeforeDecimal > maxDigitsInDecimal)
                {
                    throw new ArgumentOutOfRangeException(nameof(MaxDigitsAfterDecimal),
                        $"Sum of digits before and after decimal seperator should be less than {maxDigitsInDecimal + 1}");
                }

                this.maxDigitsAfterDecimal = value;
                this.InitKeyboard();
            }
        }

        private bool useGroupSeparator;

        public bool UseGroupSeparator
        {
            get { return this.useGroupSeparator; }
            set
            {
                this.useGroupSeparator = value;
                this.InitKeyboard();
            }
        }

        private bool numbersOnly;

        public bool NumbersOnly
        {
            get
            {
                return this.numbersOnly;
            }
            set
            {
                this.numbersOnly = value;
                this.InitKeyboard();
            }
        }

        /// <summary>
        /// <para>Occurs when numeric value changed.</para>
        /// <para>DOES NOT occur when the input is cleared.</para>
        /// </summary>
        public event EventHandler<NumericValueChangedEventArgs> NumericValueChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericEditText"/> class.
        /// </summary>
        /// <param name="context">Context</param>
        public NumericEditText(Context context)
            : base(context)
        {
            this.InitAttrs(context, null, 0);
            this.InitComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericEditText"/> class.
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="attrs">Attributes for component initialization</param>
        public NumericEditText(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            this.InitAttrs(context, attrs, 0);
            this.InitComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericEditText"/> class.
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="attrs">Attributes for component initialization</param>
        /// <param name="defStyleAttr">Style attributes for component initialization</param>
        public NumericEditText(Context context, IAttributeSet attrs, int defStyleAttr)
            : base(context, attrs, defStyleAttr)
        {
            this.InitAttrs(context, attrs, defStyleAttr);
            this.InitComponent();
        }

        private void InitAttrs(Context context, IAttributeSet attrs, int defStyleAttr)
        {
            TypedArray attributes = context.ObtainStyledAttributes(attrs, Resource.Styleable.NumericEditText, defStyleAttr, 0);

            try
            {
                this.maxDigitsBeforeDecimal = attributes.GetInt(Resource.Styleable.NumericEditText_maxDigitsBeforeDecimal, 0);
                this.maxDigitsAfterDecimal = attributes.GetInt(Resource.Styleable.NumericEditText_maxDigitsAfterDecimal, 2);
                this.numbersOnly = attributes.GetBoolean(Resource.Styleable.NumericEditText_numbersOnly, false);
                this.useGroupSeparator = attributes.GetBoolean(Resource.Styleable.NumericEditText_useGroupSeparator, true);

            }
            finally
            {
                attributes.Recycle();
            }
        }

        private void InitComponent()
        {
            this.AfterTextChanged += NumericEditText_AfterTextChanged;
            this.InitKeyboard();
        }

        private void InitKeyboard()
        {
            this.KeyListener = new CustomKeyListener(
                isDecimal: !this.NumbersOnly,
                maxDigitsBeforeDecimal: this.MaxDigitsBeforeDecimal,
                maxDigitsAfterDecimal: this.MaxDigitsAfterDecimal);
        }

        private void NumericEditText_AfterTextChanged(object sender, AfterTextChangedEventArgs e)
        {
            this.NumericValueChanged?.Invoke(this, new NumericValueChangedEventArgs(this.GetValue()));

            var enteredText = e.Editable.ToString();
            var hasTextNegativeSign = enteredText.StartsWith(this.negativeSign);
            string textWithoutSign = hasTextNegativeSign ? enteredText.Length == 1 ? "" : enteredText.Substring(1, enteredText.Length - 1) : enteredText;

            this.SetTextImpl(hasTextNegativeSign
                ? $"{this.negativeSign}{this.Format(textWithoutSign)}"
                : this.Format(textWithoutSign));
        }

        private void SetTextImpl(string text)
        {
            this.AfterTextChanged -= this.NumericEditText_AfterTextChanged;
            this.Text = text;
            this.SetSelection(text.Length);
            this.AfterTextChanged += this.NumericEditText_AfterTextChanged;
        }

        /// <summary>
        /// Gets the decimal value represented by the text. Throw if number is invalid
        /// </summary>
        /// <returns>The decimal value represented by the text</returns>
        public decimal? GetValue()
        {
            decimal value;

            if (decimal.TryParse(this.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out value))
                return value;

            return null;
        }

        public void SetValue(decimal? value)
        {
            this.SetTextImpl(this.useGroupSeparator ? value.FormatDecimal() : value?.ToString(CultureInfo.CurrentCulture) ?? "");
        }

        private string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        private string Format(string original)
        {
            string[] parts = original.Split(this.decimalSeparator.ToCharArray());
            String number = Regex.Replace(parts[0], this.numberFilterRegex, "");
            number = this.ReplaceFirst(number, leadingZeroFilterRegex, "");

            if (this.useGroupSeparator)
            {
                number = this.Reverse(Regex.Replace(this.Reverse(number), "(.{3})", "$1" + this.groupingSeparator));
                number = this.RemoveStart(number, this.groupingSeparator);
            }

            if (parts.Length > 1)
            {
                number += this.decimalSeparator + parts[1];
            }

            return number;
        }

        private string Reverse(string original)
        {
            if (original == null || original.Length <= 1)
            {
                return original;
            }
            return TextUtils.GetReverse(original, 0, original.Length);
        }

        private string RemoveStart(string str, string remove)
        {
            if (TextUtils.IsEmpty(str))
            {
                return str;
            }
            if (str.StartsWith(remove))
            {
                return str.Substring(remove.Length);
            }
            return str;
        }
    }
}