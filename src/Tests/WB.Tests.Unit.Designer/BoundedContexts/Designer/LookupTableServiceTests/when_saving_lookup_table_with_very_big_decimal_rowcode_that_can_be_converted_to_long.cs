using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Resources;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.LookupTableServiceTests
{
    internal class when_saving_lookup_table_with_very_big_decimal_rowcode_that_can_be_converted_to_long
    {
        [NUnit.Framework.Test] public void should_throw_ArgumentException () {
            fileContent =
                $"rowcode{_}min{_}max{_end}" +
                $"9223372036854775808{_}1{_}10{_end}"+
                $"10{_}50{_}100{_end}";

            lookupTableService = Create.LookupTableService();
            exception = Assert.Throws<ArgumentException>(() =>
                lookupTableService.SaveLookupTableContent(questionnaireId, lookupTableId, fileContent));

            exception.Message.Should().Be(string.Format(ExceptionMessages.LookupTables_rowcode_value_cannot_be_parsed, "9223372036854775808", "rowcode", 1));
        }


        private static Exception exception;

        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid lookupTableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static string fileContent;
        private static LookupTableService lookupTableService;

        private static string _ = "\t";
        private static string _end = "\n";
    }
}
