using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Utils.NumericTextFormatterTests
{
    public class when_filter_formatted_decimal : NumericTextFormatterTestsContext
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
                UseGroupSeparator = true
            };

            formatter = CreateNumericTextFormatter(numericTextFormatterSettings);
        };

        Because of = () => filteredResults =
            filterFormattedParams.Select(x => formatter.FilterFormatted(x.AddedText, x.SourceText, x.InsertToIndex));

        It should_return_null_for_each_validation = () =>
            filteredResults.ShouldEachConformTo(filterResult => filterResult == null);
        
        static NumericTextFormatter formatter;

        private static IEnumerable<string> filteredResults;
        private static readonly FilterFormattedParam[] filterFormattedParams =
        {
            new FilterFormattedParam { SourceText = "", AddedText = "-", InsertToIndex = 0},
            new FilterFormattedParam { SourceText = "-1", AddedText = "1", InsertToIndex = 2},
            new FilterFormattedParam { SourceText = "-1.11", AddedText = "1", InsertToIndex = 4},
            new FilterFormattedParam { SourceText = "1111", AddedText = "1", InsertToIndex = 3},
            new FilterFormattedParam { SourceText = "-1,111", AddedText = ".", InsertToIndex = 6},
            new FilterFormattedParam { SourceText = "-1,111", AddedText = "1", InsertToIndex = 2},
        };
    }
}