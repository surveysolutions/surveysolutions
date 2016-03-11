using System;
using System.Linq;

using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.Infrastructure.PlainStorage;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.LookupTableServiceTests
{
    internal class when_saving_lookup_table_with_decimal_rowcode_that_can_be_converted_to_long
    {
        Establish context = () =>
        {
            LookupTableContentStorageMock
                .Setup(x => x.Store(Moq.It.IsAny<LookupTableContent>(), Moq.It.IsAny<string>()))
                .Callback((LookupTableContent content, string id) => { lookupTableContent = content; });

            fileContent =
                $"no{_}rowcode{_}column{_end}" +
                $"3{_}4.00{_}9.087e-10{_end}";

            lookupTableService = Create.LookupTableService(lookupTableContentStorage: LookupTableContentStorageMock.Object);
        };

        Because of = () =>
            lookupTableService.SaveLookupTableContent(questionnaireId, lookupTableId, lookupTableName, fileContent);

        It should_save_not_null_content = () =>
            lookupTableContent.ShouldNotBeNull();

        It should_save_row_with_rowcode__4 = () =>
            lookupTableContent.Rows.ElementAt(0).RowCode.ShouldEqual(4);

        It should_save_row_with_data__9_087e_10 = () =>
            lookupTableContent.Rows.ElementAt(0).Variables[1].ShouldEqual((decimal)9.087E-10);

        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid lookupTableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly string lookupTableName = "lookup";
        private static string fileContent;
        private static LookupTableContent lookupTableContent;
        private static LookupTableService lookupTableService;
        private static readonly Mock<IPlainKeyValueStorage<LookupTableContent>> LookupTableContentStorageMock = new Mock<IPlainKeyValueStorage<LookupTableContent>>();

        private static string _ = "\t";
        private static string _end = "\n";
    }
}