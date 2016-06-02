using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Utils.NumericTextFormatterTests
{
    public class when_filter_formatted_integer_with_errors : NumericTextFormatterTestsContext
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
                MaxDigitsBeforeDecimal = 13,
                UseGroupSeparator = true
            };

            formatter = CreateNumericTextFormatter(numericTextFormatterSettings);
        };

        Because of = () => filteredResults =
            filterFormattedParams.Select(x => formatter.FilterFormatted(x.AddedText, x.SourceText, x.InsertToIndex));

        It should_return_empty_string_for_each_validation = () =>
            filteredResults.ShouldEachConformTo(filterResult => filterResult == "");
        
        static NumericTextFormatter formatter;

        private static IEnumerable<string> filteredResults;
        private static readonly FilterFormattedParam[] filterFormattedParams =
        {
            new FilterFormattedParam { SourceText = "1", AddedText = "-", InsertToIndex = 1},
            new FilterFormattedParam { SourceText = "1", AddedText = ")", InsertToIndex = 1},
            new FilterFormattedParam { SourceText = "11", AddedText = ".", InsertToIndex = 2},
            new FilterFormattedParam { SourceText = "-1", AddedText = "-", InsertToIndex = 2},
            new FilterFormattedParam { SourceText = "1,111", AddedText = ",", InsertToIndex = 1},
            new FilterFormattedParam { SourceText = "1111111111111111111111111111", AddedText = "1", InsertToIndex = 28},
        };
    }
}