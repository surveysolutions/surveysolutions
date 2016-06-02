using Machine.Specifications;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Utils.NumericTextFormatterTests
{
    public class when_formatting_and_use_group_formatting_is_false : NumericTextFormatterTestsContext
    {
        Establish context = () =>
        {
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
        };

        Because of = () => actualResult = formatter.Format(expectedResult);

        It should_return_source_text_without_modifications = () =>
            actualResult.ShouldEqual(expectedResult);
        
        static NumericTextFormatter formatter;
        const string expectedResult = "-55555555";

        private static string actualResult;
    }
}