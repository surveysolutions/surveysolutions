using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Resources;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.LookupTableServiceTests
{
    internal class when_saving_lookup_table_without_rowcode_column
    {
        [NUnit.Framework.Test] public void should_throw_ArgumentException () {
            fileContent =
                $"no{_}row{_}code{_}column{_end}" +
                $"1{_}2{_}3{_}4{_end}";

            lookupTableService = Create.LookupTableService();
            exception = Assert.Throws<ArgumentException>(() =>
                lookupTableService.SaveLookupTableContent(questionnaireId, lookupTableId, fileContent));

            exception.Message.Should().Be(ExceptionMessages.LookupTables_rowcode_column_is_mandatory);
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
