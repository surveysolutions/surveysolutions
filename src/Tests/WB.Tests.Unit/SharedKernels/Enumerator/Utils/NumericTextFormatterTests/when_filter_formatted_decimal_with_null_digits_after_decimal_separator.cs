using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Utils.NumericTextFormatterTests
{
    public class when_filter_formatted_decimal_with_null_digits_after_decimal_separator : NumericTextFormatterTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var numericTextFormatterSettings = new NumericTextFormatterSettings
            {
                IsDecimal = true,
                NegativeSign = "-",
                DecimalSeparator = ".",
                GroupingSeparator = ",",
                MaxDigitsAfterDecimal = null,
                MaxDigitsBeforeDecimal = 15,
                UseGroupSeparator = true
            };

            formatter = CreateNumericTextFormatter(numericTextFormatterSettings);
            BecauseOf();
        }

        private void BecauseOf() => filteredResults =
            FilterFormattedParams.Select(x => formatter.FilterFormatted(x.AddedText, x.SourceText, x.InsertToIndex));

        [NUnit.Framework.Test]
        public void should_return_null_for_each_validation()
        {
            foreach (var filteredResult in filteredResults)
            {
                filteredResult.Should().BeNull();
            }
        }

        static NumericTextFormatter formatter;

        private static IEnumerable<string> filteredResults;

        private static readonly FilterFormattedParam[] FilterFormattedParams =
        new []{
            new FilterFormattedParam {SourceText = "", AddedText = "-", InsertToIndex = 0},
            new FilterFormattedParam {SourceText = "-1", AddedText = "1", InsertToIndex = 2},
            new FilterFormattedParam {SourceText = "-1.11", AddedText = "1", InsertToIndex = 4},
            new FilterFormattedParam {SourceText = "1111", AddedText = "1", InsertToIndex = 3},
            new FilterFormattedParam {SourceText = "-1,111", AddedText = ".", InsertToIndex = 6},
            new FilterFormattedParam {SourceText = "-1,111", AddedText = "1", InsertToIndex = 2},
            new FilterFormattedParam {SourceText = "11111111111111", AddedText = ".", InsertToIndex = 13},
            new FilterFormattedParam {SourceText = "1111111111111111111111", AddedText = ".", InsertToIndex = 13}
        };
    }
}
