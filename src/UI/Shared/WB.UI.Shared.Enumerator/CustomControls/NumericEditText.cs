using System.Globalization;
using Android.Content;
using Android.Content.Res;
using Android.Text;
using Android.Text.Method;
using Android.Util;
using Java.Lang;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Utils;
using Math = System.Math;

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
            private readonly NumericTextFormatter numericTextFormatter;
        
            public CustomKeyListener(NumericTextFormatter numericTextFormatter, bool @decimal) : base(true, @decimal)
            {
                this.numericTextFormatter = numericTextFormatter;
            }

            public override ICharSequence FilterFormatted(ICharSequence source, int start, int end, ISpanned dest, int dstart, int dend)
            {
                var filteredText = this.numericTextFormatter.FilterFormatted(source.ToString(), dest.ToString(), dstart);
                return filteredText == null ? null : new Java.Lang.String(filteredText);
            }
        }

        private const int MaxDigitsInDecimal = 28;
        private const int MaxFractionDigits = 15;

        private int maxDigitsBeforeDecimal;
        /// <summary>
        /// Gets or sets the maximum number of digits before the decimal point.
        /// </summary>
        /// <value>The maximum number of digits before the decimal point</value>
        public int MaxDigitsBeforeDecimal
        {
            get => this.maxDigitsBeforeDecimal;
            set
            {
                if (value + (MaxDigitsAfterDecimal ?? 0)> MaxDigitsInDecimal)
                {
                    throw new ArgumentOutOfRangeException(nameof(MaxDigitsBeforeDecimal),
                        $"Sum of digits before and after decimal seperator should be less than {MaxDigitsInDecimal + 1}");
                }

                this.maxDigitsBeforeDecimal = value;
                this.InitKeyboard();
            }
        }

        private int? maxDigitsAfterDecimal;

        /// <summary>
        /// Gets or sets the maximum number of digits after the decimal point.
        /// </summary>
        /// <value>The maximum number of digits after the decimal point</value>
        public int? MaxDigitsAfterDecimal
        {
            get => this.maxDigitsAfterDecimal;
            set
            {
                if (value > MaxFractionDigits)
                {
                    throw new ArgumentOutOfRangeException(nameof(MaxDigitsAfterDecimal),
                        $"Number of digits after decimal seperator should be less than {MaxFractionDigits + 1}");
                }
                if (value.HasValue && value.Value + this.maxDigitsBeforeDecimal > MaxDigitsInDecimal)
                {
                    throw new ArgumentOutOfRangeException(nameof(MaxDigitsAfterDecimal),
                        $"Sum of digits before and after decimal seperator should be less than {MaxDigitsInDecimal + 1}");
                }

                this.maxDigitsAfterDecimal = value;
                this.InitKeyboard();
            }
        }

        private bool useGroupSeparator;

        public bool UseGroupSeparator
        {
            get => this.useGroupSeparator;
            set
            {
                this.useGroupSeparator = value;
                this.InitKeyboard();
            }
        }

        private bool numbersOnly;

        public bool NumbersOnly
        {
            get => this.numbersOnly;
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
            var androidNumericTextFormatterSettings = new NumericTextFormatterSettings
            {
                DecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator,
                NegativeSign = CultureInfo.CurrentCulture.NumberFormat.NegativeSign,
                GroupingSeparator = CultureInfo.CurrentCulture.NumberFormat.CurrencyGroupSeparator,
                IsDecimal = !this.NumbersOnly,
                MaxDigitsAfterDecimal = this.MaxDigitsAfterDecimal,
                MaxDigitsBeforeDecimal = this.MaxDigitsBeforeDecimal,
                UseGroupSeparator = this.UseGroupSeparator
            };

            this.numericTextFormatter = new NumericTextFormatter(androidNumericTextFormatterSettings);
            this.KeyListener = new CustomKeyListener(this.numericTextFormatter, !this.NumbersOnly);
        }

        private void NumericEditText_AfterTextChanged(object sender, AfterTextChangedEventArgs e)
        {
            this.NumericValueChanged?.Invoke(this, new NumericValueChangedEventArgs(this.GetValue()));

            this.SetTextImpl(this.numericTextFormatter.Format(e.Editable.ToString()));
        }

        private string previousText;
        private NumericTextFormatter numericTextFormatter;

        private void SetTextImpl(string text)
        {
            var newSelectionStart = this.SelectionStart;
            var newTextLength = text?.Length ?? 0;

            this.AfterTextChanged -= this.NumericEditText_AfterTextChanged;
            this.Text = text;
            
            var numberOfInsertedOrDeletedChars = newTextLength - (this.previousText?.Length ?? 0);
            if (numberOfInsertedOrDeletedChars != 0)
            {
                var cursorPositionAdjustment = numberOfInsertedOrDeletedChars > 0 ? -1 : 1;
                newSelectionStart += numberOfInsertedOrDeletedChars + cursorPositionAdjustment;
                newSelectionStart = Math.Max(newSelectionStart, 0);
            }
            
            this.SetSelection(Math.Min(newSelectionStart, newTextLength));

            this.previousText = text;
            this.AfterTextChanged += this.NumericEditText_AfterTextChanged;
        }

        /// <summary>
        /// Gets the decimal value represented by the text. Throw if number is invalid
        /// </summary>
        /// <returns>The decimal value represented by the text</returns>
        public decimal? GetValue()
        {
            if (decimal.TryParse(this.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out var value))
                return value;

            return null;
        }

        public void SetValue(decimal? value)
        {
            this.SetTextImpl(this.useGroupSeparator ? value.FormatDecimal() : value?.ToString(CultureInfo.CurrentCulture) ?? "");
        }
    }
}
