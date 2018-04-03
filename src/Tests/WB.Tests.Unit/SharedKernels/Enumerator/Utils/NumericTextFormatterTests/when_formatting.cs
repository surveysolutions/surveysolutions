using FluentAssertions;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Utils.NumericTextFormatterTests
{
    public class when_formatting : NumericTextFormatterTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var numericTextFormatterSettings = new NumericTextFormatterSettings
            {
                IsDecimal = true,
                NegativeSign = "-",
                DecimalSeparator = ",",
                GroupingSeparator = ".",
                MaxDigitsAfterDecimal = 15,
                MaxDigitsBeforeDecimal = 13,
                UseGroupSeparator = true
            };

            formatter = CreateNumericTextFormatter(numericTextFormatterSettings);
            BecauseOf();
        }

        public void BecauseOf() => actualResult = formatter.Format("-1234567,89");

        [NUnit.Framework.Test] public void should_return_formatted_decimal_with_groups_separator () =>
            actualResult.Should().Be("-1.234.567,89");
        
        static NumericTextFormatter formatter;

        private static string actualResult;
    }
}
