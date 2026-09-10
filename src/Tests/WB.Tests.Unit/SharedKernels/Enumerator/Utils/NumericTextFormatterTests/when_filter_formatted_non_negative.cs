using FluentAssertions;
using NUnit.Framework;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Utils.NumericTextFormatterTests
{
    public class when_filter_formatted_non_negative : NumericTextFormatterTestsContext
    {
        [Test]
        public void should_reject_localized_and_pasted_negative_values()
        {
            var integerFormatter = CreateNonNegativeIntegerFormatter();
            var decimalFormatter = CreateNonNegativeDecimalFormatter();

            integerFormatter.FilterFormatted("−", "", 0).Should().BeEmpty();
            integerFormatter.FilterFormatted("−1", "", 0).Should().BeEmpty();
            decimalFormatter.FilterFormatted("−5,2", "", 0).Should().BeEmpty();
        }

        [Test]
        public void should_accept_positive_integer_and_decimal_values()
        {
            var integerFormatter = CreateNonNegativeIntegerFormatter();
            var decimalFormatter = CreateNonNegativeDecimalFormatter();

            integerFormatter.FilterFormatted("5", "", 0).Should().BeNull();
            decimalFormatter.FilterFormatted("5,2", "", 0).Should().BeNull();
        }

        private static NumericTextFormatter CreateNonNegativeIntegerFormatter() =>
            CreateNumericTextFormatter(new NumericTextFormatterSettings
            {
                IsDecimal = false,
                NegativeSign = "−",
                DecimalSeparator = ".",
                GroupingSeparator = ",",
                MaxDigitsAfterDecimal = 15,
                MaxDigitsBeforeDecimal = 13,
                UseGroupSeparator = true,
                IsNonNegative = true
            });

        private static NumericTextFormatter CreateNonNegativeDecimalFormatter() =>
            CreateNumericTextFormatter(new NumericTextFormatterSettings
            {
                IsDecimal = true,
                NegativeSign = "−",
                DecimalSeparator = ",",
                GroupingSeparator = ".",
                MaxDigitsAfterDecimal = 15,
                MaxDigitsBeforeDecimal = 13,
                UseGroupSeparator = true,
                IsNonNegative = true
            });
    }
}
