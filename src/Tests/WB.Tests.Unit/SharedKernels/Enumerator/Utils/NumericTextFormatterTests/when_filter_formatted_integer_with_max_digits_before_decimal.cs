using FluentAssertions;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Utils.NumericTextFormatterTests
{
    public class when_filter_formatted_integer_with_max_digits_before_decimal : NumericTextFormatterTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var numericTextFormatterSettings = new NumericTextFormatterSettings
            {
                IsDecimal = false,
                NegativeSign = "-",
                DecimalSeparator = ".",
                GroupingSeparator = ",",
                MaxDigitsAfterDecimal = 15,
                MaxDigitsBeforeDecimal = 2,
                UseGroupSeparator = true
            };

            formatter = CreateNumericTextFormatter(numericTextFormatterSettings);
            BecauseOf();
        }

        public void BecauseOf() => filteredResult = formatter.FilterFormatted(addedText: "1", sourceText: "11", insertToIndex: 2);

        [NUnit.Framework.Test] public void should_return_empty_string () =>
            filteredResult.Should().Be("");
        
        static NumericTextFormatter formatter;

        private static string filteredResult;
    }
}
