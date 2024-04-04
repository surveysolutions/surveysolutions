using FluentAssertions;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Utils.NumericTextFormatterTests
{
    public class when_filter_formatted_decimal_with_zero_digits_after_decimal_separator : NumericTextFormatterTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var numericTextFormatterSettings = new NumericTextFormatterSettings
            {
                IsDecimal = true,
                NegativeSign = "-",
                DecimalSeparator = ".",
                GroupingSeparator = ",",
                MaxDigitsAfterDecimal = 0,
                MaxDigitsBeforeDecimal = 15,
                UseGroupSeparator = true
            };

            formatter = CreateNumericTextFormatter(numericTextFormatterSettings);
            BecauseOf();
        }

        public void BecauseOf() => filteredResult = formatter.FilterFormatted(addedText: ".", sourceText: "11114", insertToIndex: 5);

        [NUnit.Framework.Test] public void should_return_empty_string () =>
            filteredResult.Should().Be("");
        
        static NumericTextFormatter formatter;

        private static string filteredResult;
    }
}
