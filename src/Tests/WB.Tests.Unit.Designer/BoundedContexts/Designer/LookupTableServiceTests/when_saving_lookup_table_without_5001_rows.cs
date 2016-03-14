using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Resources;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.LookupTableServiceTests
{
    internal class when_saving_lookup_table_without_5001_rows
    {
        Establish context = () =>
        {
            fileContent = $"no{_}rowcode{_}column{_end}";
            for (int i = 0; i < 5001; i++)
            {
                fileContent += $"1{_}2{_}3{_end}";
            }

            lookupTableService = Create.LookupTableService();
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                lookupTableService.SaveLookupTableContent(questionnaireId, lookupTableId, lookupTableName, fileContent));

        It should_throw_exception = () =>
            exception.ShouldNotBeNull();

        It should_throw_ArgumentException = () =>
            exception.ShouldBeOfExactType<ArgumentException>();

        It should_throw_ArgumentException1 = () =>
            ((ArgumentException)exception).Message.ShouldEqual(string.Format(ExceptionMessages.LookupTables_too_many_rows, 5000));

        private static Exception exception;

        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid lookupTableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly string lookupTableName = "lookup";
        private static string fileContent;
        private static LookupTableService lookupTableService;

        private static string _ = "\t";
        private static string _end = "\n";
    }
}