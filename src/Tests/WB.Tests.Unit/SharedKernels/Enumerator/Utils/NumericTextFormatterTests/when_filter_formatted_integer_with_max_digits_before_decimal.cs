using Machine.Specifications;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Utils.NumericTextFormatterTests
{
    public class when_filter_formatted_integer_with_max_digits_before_decimal : NumericTextFormatterTestsContext
    {
        Establish context = () =>
        {
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
        };

        Because of = () => filteredResult = formatter.FilterFormatted(addedText: "1", sourceText: "11", insertToIndex: 2);

        It should_return_empty_string = () =>
            filteredResult.ShouldEqual("");
        
        static NumericTextFormatter formatter;

        private static string filteredResult;
    }
}