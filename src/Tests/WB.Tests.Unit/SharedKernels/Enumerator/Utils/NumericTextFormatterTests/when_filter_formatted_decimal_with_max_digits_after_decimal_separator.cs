using Machine.Specifications;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Utils.NumericTextFormatterTests
{
    public class when_filter_formatted_decimal_with_max_digits_after_decimal_separator : NumericTextFormatterTestsContext
    {
        Establish context = () =>
        {
            var numericTextFormatterSettings = new NumericTextFormatterSettings
            {
                IsDecimal = true,
                NegativeSign = "-",
                DecimalSeparator = ".",
                GroupingSeparator = ",",
                MaxDigitsAfterDecimal = 2,
                MaxDigitsBeforeDecimal = 13,
                UseGroupSeparator = true
            };

            formatter = CreateNumericTextFormatter(numericTextFormatterSettings);
        };

        Because of = () => filteredResult = formatter.FilterFormatted(addedText: "1", sourceText: "11.11", insertToIndex: 5);

        It should_return_empty_string = () =>
            filteredResult.ShouldEqual("");
        
        static NumericTextFormatter formatter;

        private static string filteredResult;
    }
}