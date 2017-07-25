using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Resources;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.LookupTableServiceTests
{
    internal class when_saving_lookup_table_with_non_integer_rowcode_value
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            fileContent = 
                $"no{_}rowcode{_}column{_end}" + 
                $"1{_}2{_}3{_end}" + 
                $"2{_}3{_}4{_end}" + 
                $"3{_}4{_}5{_end}" + 
                $"4{_}non_integer{_}3{_end}";

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
            ((ArgumentException)exception).Message.ShouldEqual(string.Format(ExceptionMessages.LookupTables_rowcode_value_cannot_be_parsed, "non_integer", "rowcode", 4));

        private static Exception exception;

        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid lookupTableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static string fileContent;
        private static LookupTableService lookupTableService;

        private static string _ = "\t";
        private static string _end = "\n";
    }
}