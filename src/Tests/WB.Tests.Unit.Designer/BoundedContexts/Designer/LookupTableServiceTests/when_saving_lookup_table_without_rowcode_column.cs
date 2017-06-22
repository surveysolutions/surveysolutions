using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Resources;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.LookupTableServiceTests
{
    internal class when_saving_lookup_table_without_rowcode_column
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            fileContent =
                $"no{_}row{_}code{_}column{_end}" +
                $"1{_}2{_}3{_}4{_end}";

            lookupTableService = Create.LookupTableService();
            BecauseOf();
        }

        private void BecauseOf() =>
            exception = Catch.Exception(() =>
                lookupTableService.SaveLookupTableContent(questionnaireId, lookupTableId, fileContent));

        [NUnit.Framework.Test] public void should_throw_exception () =>
            exception.ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_throw_ArgumentException () =>
            exception.ShouldBeOfExactType<ArgumentException>();

        [NUnit.Framework.Test] public void should_throw_ArgumentException1 () =>
            ((ArgumentException)exception).Message.ShouldEqual(ExceptionMessages.LookupTables_rowcode_column_is_mandatory);

        private static Exception exception;

        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid lookupTableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static string fileContent;
        private static LookupTableService lookupTableService;

        private static string _ = "\t";
        private static string _end = "\n";
    }
}