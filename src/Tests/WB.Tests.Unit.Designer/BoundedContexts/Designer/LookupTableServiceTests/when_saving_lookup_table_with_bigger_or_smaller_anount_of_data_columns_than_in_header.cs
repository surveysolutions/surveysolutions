using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.Infrastructure.PlainStorage;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.LookupTableServiceTests
{
    internal class when_saving_lookup_table_with_bigger_or_smaller_anount_of_data_columns_than_in_header
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            LookupTableContentStorageMock
                .Setup(x => x.Store(Moq.It.IsAny<LookupTableContent>(), Moq.It.IsAny<string>()))
                .Callback((LookupTableContent content, string id) => { lookupTableContent = content; });

            fileContent =
                $"no{_}rowcode{_}column{_end}" +
                $"3{_}4{_}5{_}5.5{_end}" +
                $"6{_}7{_end}";

            lookupTableService = Create.LookupTableService(lookupTableContentStorage: LookupTableContentStorageMock.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            lookupTableService.SaveLookupTableContent(questionnaireId, lookupTableId, fileContent);

        [NUnit.Framework.Test] public void should_save_not_null_content () =>
            lookupTableContent.ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_save_first_row_with_2_values_as_in_header () =>
            lookupTableContent.Rows.ElementAt(0).Variables.Length.ShouldEqual(3 - 1);

        [NUnit.Framework.Test] public void should_save_first_row_with_last_value_5 () =>
            lookupTableContent.Rows.ElementAt(0).Variables.Last().ShouldEqual(5);

        [NUnit.Framework.Test] public void should_save_second_row_with_2_values_as_in_header () =>
            lookupTableContent.Rows.ElementAt(1).Variables.Length.ShouldEqual(3 - 1);

        [NUnit.Framework.Test] public void should_save_second_row_with_last_value__null () =>
            lookupTableContent.Rows.ElementAt(1).Variables.Last().ShouldBeNull();

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