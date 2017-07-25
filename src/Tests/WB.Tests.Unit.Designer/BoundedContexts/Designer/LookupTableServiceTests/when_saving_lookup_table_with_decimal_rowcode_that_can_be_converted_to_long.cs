using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.Infrastructure.PlainStorage;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.LookupTableServiceTests
{
    internal class when_saving_lookup_table_with_decimal_rowcode_that_can_be_converted_to_long
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            LookupTableContentStorageMock
                .Setup(x => x.Store(Moq.It.IsAny<LookupTableContent>(), Moq.It.IsAny<string>()))
                .Callback((LookupTableContent content, string id) => { lookupTableContent = content; });

            fileContent =
                $"no{_}rowcode{_}column{_end}" +
                $"3{_}4.00{_}9.087e-10{_end}";

            lookupTableService = Create.LookupTableService(lookupTableContentStorage: LookupTableContentStorageMock.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            lookupTableService.SaveLookupTableContent(questionnaireId, lookupTableId, fileContent);

        [NUnit.Framework.Test] public void should_save_not_null_content () =>
            lookupTableContent.ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_save_row_with_rowcode__4 () =>
            lookupTableContent.Rows.ElementAt(0).RowCode.ShouldEqual(4);

        [NUnit.Framework.Test] public void should_save_row_with_data__9_087e_10 () =>
            lookupTableContent.Rows.ElementAt(0).Variables[1].ShouldEqual((decimal)9.087E-10);

        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid lookupTableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static string fileContent;
        private static LookupTableContent lookupTableContent;
        private static LookupTableService lookupTableService;
        private static readonly Mock<IPlainKeyValueStorage<LookupTableContent>> LookupTableContentStorageMock = new Mock<IPlainKeyValueStorage<LookupTableContent>>();

        private static string _ = "\t";
        private static string _end = "\n";
    }
}