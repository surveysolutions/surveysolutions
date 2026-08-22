namespace WB.Core.SharedKernels.Enumerator.Utils
{
    public class NumericTextFormatterSettings
    {
        public string GroupingSeparator { get; set; }
        public string DecimalSeparator { get; set; }
        public string NegativeSign { get; set; }
        
        public bool IsDecimal { get; set; }
        public int MaxDigitsBeforeDecimal { get; set; }
        public int? MaxDigitsAfterDecimal { get; set; }
        public bool UseGroupSeparator { get; set; }

        /// <summary>
        /// When true, the minus/negative sign is not allowed to be entered.
        /// </summary>
        public bool IsNonNegative { get; set; }
    }
}
