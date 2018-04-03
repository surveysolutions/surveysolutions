using FluentAssertions;
using NUnit.Framework;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Utils.NumericTextFormatterTests
{
    public class when_filter_formatted_and_added_text_contains_non_localized_android_decimal_separator : NumericTextFormatterTestsContext
    {
        [Test]
        public void should_return_empty_string()
        {
            var numericTextFormatterSettings = new NumericTextFormatterSettings
            {
                IsDecimal = true,
                NegativeSign = "-",
                DecimalSeparator = ",",
                GroupingSeparator = " ",
                MaxDigitsAfterDecimal = 15,
                MaxDigitsBeforeDecimal = 13,
                UseGroupSeparator = true
            };

            var formatter = CreateNumericTextFormatter(numericTextFormatterSettings);

            var filteredResult = formatter.FilterFormatted(addedText: ".", sourceText: "1111", insertToIndex: 4);

            filteredResult.Should().Be(",");
        }
    }
}
