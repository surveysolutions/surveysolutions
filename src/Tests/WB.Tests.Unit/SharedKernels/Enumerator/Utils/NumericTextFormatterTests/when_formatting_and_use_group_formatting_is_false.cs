using FluentAssertions;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Utils.NumericTextFormatterTests
{
    public class when_formatting_and_use_group_formatting_is_false : NumericTextFormatterTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var numericTextFormatterSettings = new NumericTextFormatterSettings
            {
                IsDecimal = true,
                NegativeSign = "-",
                DecimalSeparator = ".",
                GroupingSeparator = ",",
                MaxDigitsAfterDecimal = 15,
                MaxDigitsBeforeDecimal = 13,
                UseGroupSeparator = false
            };

            formatter = CreateNumericTextFormatter(numericTextFormatterSettings);
            BecauseOf();
        }

        public void BecauseOf() => actualResult = formatter.Format(expectedResult);

        [NUnit.Framework.Test] public void should_return_source_text_without_modifications () =>
            actualResult.Should().Be(expectedResult);
        
        static NumericTextFormatter formatter;
        const string expectedResult = "-55555555";

        private static string actualResult;
    }
}
