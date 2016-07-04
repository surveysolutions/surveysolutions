using Machine.Specifications;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Utils.NumericTextFormatterTests
{
    public class when_filter_formatted_and_added_text_contains_non_localized_android_decimal_separator : NumericTextFormatterTestsContext
    {
        Establish context = () =>
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

            formatter = CreateNumericTextFormatter(numericTextFormatterSettings);
        };

        Because of = () => filteredResult = formatter.FilterFormatted(addedText: ".", sourceText: "1111", insertToIndex: 4);

        It should_return_empty_string = () =>
            filteredResult.ShouldEqual(",");
        
        static NumericTextFormatter formatter;

        private static string filteredResult;
    }
}