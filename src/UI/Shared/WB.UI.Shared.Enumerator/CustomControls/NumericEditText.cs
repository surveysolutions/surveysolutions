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

    public class NumericValueClearedEventArgs : EventArgs { }

    /// <summary>
    /// Numeric Edit Text that supports decimal separator and group separator according to your culture.
    /// </summary>
    public class NumericEditText : EditText
    {
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
                this.SetTextInternal(this.Text);
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

                this.SetTextInternal(this.Text);
            }
        }

        private bool useGroupSeparator;

        public bool UseGroupSeparator
        {
            get { return this.useGroupSeparator; }
            set
            {
                this.useGroupSeparator = value;
                this.SetTextInternal(this.Text);
            }
        }

        public bool NumbersOnly { get; set; }

        private readonly string groupingSeparator = CultureInfo.CurrentCulture.NumberFormat.CurrencyGroupSeparator;
        private readonly string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        private readonly string negativeSign = CultureInfo.CurrentCulture.NumberFormat.NegativeSign;
        
        private readonly string numberFilterRegex = "[^\\d\\" + CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator + "]";
        private const string leadingZeroFilterRegex = "^0+(?!$)";

        private const int maxDigitsInDecimal = 16;
        private const int maxFractionDigits = 15;

        private string previousText = "";

        /// <summary>
        /// <para>Occurs when numeric value changed.</para>
        /// <para>DOES NOT occur when the input is cleared.</para>
        /// </summary>
        public event EventHandler<NumericValueChangedEventArgs> NumericValueChanged;

        /// <summary>
        /// Occurs when numeric value cleared.
        /// </summary>
        public event EventHandler<NumericValueClearedEventArgs> NumericValueCleared;

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
                this.NumbersOnly = attributes.GetBoolean(Resource.Styleable.NumericEditText_numbersOnly, false);
                this.useGroupSeparator = attributes.GetBoolean(Resource.Styleable.NumericEditText_useGroupSeparator, true);

            }
            finally
            {
                attributes.Recycle();
            }
        }

        private void InitComponent()
        {
            this.AfterTextChanged += this.TextChangedHandler;
            this.Click += (sender, e) => { SetSelection(Text.Length); };

            string allowedDigits = "0123456789" + this.negativeSign;
            if (!this.NumbersOnly)
                allowedDigits += this.decimalSeparator + this.groupingSeparator;

            this.KeyListener = DigitsKeyListener.GetInstance(allowedDigits);
        }
        
        private void TextChangedHandler(object sender, AfterTextChangedEventArgs e)
        {
            var enteredText = e.Editable.ToString();

            if (enteredText.Length == 0)
            {
                this.HandleNumericValueCleared();
                return;
            }

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
                () =>
                {
                    int decimalPointPosition = enteredText.IndexOf(this.decimalSeparator);
                    return decimalPointPosition > 0 && textWithoutSign.Substring(decimalPointPosition).IndexOf(this.groupingSeparator) > 0;
                },
                () => textWithoutSign.Length == 1 && textWithoutSign == this.decimalSeparator,
                () => this.CountMatches(enteredText, this.decimalSeparator) > 1,
                () => fraction.Length > maxFractionDigits,
                () => (integer.Length + fraction.Length) > maxDigitsInDecimal,
                () => this.MaxDigitsBeforeDecimal > 0 && integer.Length > this.MaxDigitsBeforeDecimal,
                () => this.MaxDigitsAfterDecimal > 0 && fraction.Length > this.MaxDigitsAfterDecimal,
                () =>
                {
                    if (textWithoutSign.Length <= 2) return false;

                    string lastChar = textWithoutSign[textWithoutSign.Length - 1].ToString();
                    string secToLastChar = textWithoutSign[textWithoutSign.Length - 2].ToString();
                    if (lastChar != this.decimalSeparator && lastChar != this.groupingSeparator) return false;

                    return lastChar == secToLastChar;
                }
            };

            if (varifiers.Any(isInvalid => isInvalid()))
            {
                this.DiscardInput();
                return;
            }

            enteredText = hasTextNegativeSign
                ? $"{this.negativeSign}{this.Format(textWithoutSign)}"
                : this.Format(enteredText);

            this.SetTextInternal(enteredText);

            this.SetSelection(enteredText.Length);
            this.HandleNumericValueChanged();
        }

        private void DiscardInput()
        {
            this.Text = previousText;
            this.SetSelection(Text.Length);
        }

        private void HandleNumericValueCleared()
        {
            this.previousText = "";
            this.NumericValueCleared?.Invoke(this, new NumericValueClearedEventArgs());
        }

        private void HandleNumericValueChanged()
        {
            this.previousText = this.Text;
            this.NumericValueChanged?.Invoke(this, new NumericValueChangedEventArgs(this.GetValue()));
        }

        private void SetTextInternal(string text)
        {
            this.AfterTextChanged -= this.TextChangedHandler;
            this.Text = text;
            this.AfterTextChanged += this.TextChangedHandler;
        }

        /// <summary>
        /// Gets the decimal value represented by the text. Throw if number is invalid
        /// </summary>
        /// <returns>The decimal value represented by the text</returns>
        public decimal? GetValue()
        {
            bool hasSign = this.Text.StartsWith(this.negativeSign);
            string original = Regex.Replace(this.Text, this.numberFilterRegex, "");

            return string.IsNullOrEmpty(original) ? (decimal?)null : decimal.Parse(hasSign ? $"{this.negativeSign}{original}" : original);
        }

        public void SetValue(decimal? value)
        {
            var valueAsString = value?.ToString(CultureInfo.CurrentCulture).Replace($"{this.decimalSeparator}0", "") ?? "";

            this.SetTextInternal(this.Format(valueAsString));
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

            if (this.UseGroupSeparator)
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
}