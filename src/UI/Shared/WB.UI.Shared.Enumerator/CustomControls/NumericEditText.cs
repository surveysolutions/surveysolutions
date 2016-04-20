using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Android.Content;
using Android.Content.Res;
using Android.Text;
using Android.Text.Method;
using Android.Util;
using Android.Widget;
using Java.Text;

namespace WB.UI.Shared.Enumerator.CustomControls
{
    public class NumericValueChangedEventArgs : EventArgs
    {
        public decimal NewValue { get; set; }

        public NumericValueChangedEventArgs(decimal newValue)
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
        private string previousText = "";
        private string numberFilterRegex = "";
        private const string leadingZeroFilterRegex = "^0+(?!$)";
        private const int maxDigitsInDecimal = 28;

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
            this.numberFilterRegex = "[^\\d\\" + this.decimalSeparator + "]";
            this.AfterTextChanged += this.TextChangedHandler;
            this.Click += (sender, e) => { SetSelection(Text.Length); };
            this.KeyListener = DigitsKeyListener.GetInstance(true, !this.NumbersOnly);
        }

        private void TextChangedHandler(object sender, AfterTextChangedEventArgs e)
        {
            var editText = e.Editable.ToString();
            
            var countOfNegativeSigns = this.CountMatches(editText, this.negativeSign);
            var hasSign = editText.StartsWith(this.negativeSign);

            if (countOfNegativeSigns > 1 || (countOfNegativeSigns == 1 && !hasSign))
            {
                this.DiscardInput();
                return;
            }
            
            string newText = hasSign ? editText.Length == 1 ? "" : editText.Substring(1, editText.Length-1) : editText;

            int decimalPointPosition = editText.IndexOf(this.decimalSeparator);
            if (decimalPointPosition > 0)
            {
                if (newText.Substring(decimalPointPosition).IndexOf(this.groupingSeparator) > 0)
                {
                    this.DiscardInput();
                    return;
                }
            }

            if (newText.Length == 1 && newText == this.decimalSeparator)
            {
                this.DiscardInput();
                return;
            }

            string[] splitText = newText.Split(this.decimalSeparator.ToCharArray());
            string leftPart = (splitText[0] ?? "").Replace(this.groupingSeparator, "");
            string rightPart = "";
            if (splitText.Length > 1)
            {
                rightPart = splitText[1] ?? "";
            }

            if((leftPart.Length + rightPart.Length) > maxDigitsInDecimal)
            {
                this.DiscardInput();
                return;
            }

            if (this.MaxDigitsBeforeDecimal > 0 && leftPart.Length > this.MaxDigitsBeforeDecimal)
            {
                this.DiscardInput();
                return;
            }

            if (this.MaxDigitsAfterDecimal > 0 && rightPart.Length > this.MaxDigitsAfterDecimal)
            {
                this.DiscardInput();
                return;
            }

            if (newText.Length > 2)
            {
                string lastChar = newText[newText.Length - 1].ToString();
                string secToLastChar = newText[newText.Length - 2].ToString();
                if (lastChar == this.decimalSeparator || lastChar == this.groupingSeparator)
                {
                    if (lastChar == secToLastChar)
                    {
                        this.DiscardInput();
                        return;
                    }
                }
            }

            if (this.CountMatches(editText, this.decimalSeparator) > 1)
            {
                this.DiscardInput();
                return;
            }

            if (e.Editable.Length() == 0)
            {
                this.HandleNumericValueCleared();
                return;
            }

            this.SetTextInternal(hasSign ? $"{this.negativeSign}{this.Format(newText)}" : this.Format(editText));
            this.SetSelection(Text.Length);
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
            var handler = this.NumericValueCleared;
            handler?.Invoke(this, new NumericValueClearedEventArgs());
        }

        private void HandleNumericValueChanged()
        {
            this.previousText = this.Text;
            var handler = this.NumericValueChanged;
            handler?.Invoke(this, new NumericValueChangedEventArgs(this.GetNumericValue()));
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
        public decimal GetNumericValue()
        {
            bool hasSign = this.Text.StartsWith(this.negativeSign); 
            string original = Regex.Replace(this.Text, this.numberFilterRegex, "");

            return decimal.Parse(hasSign ? this.negativeSign + original : original);
        }

        public void SetValue(decimal? value)
        {
            this.Text = value?.ToString(CultureInfo.CurrentCulture).Replace($"{this.decimalSeparator}0", "");
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