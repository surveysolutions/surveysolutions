using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    internal class when_CreatePreloadedDataDtosFromPanelData_is_called_for_2_data_files : PreloadedDataServiceTestContext
    {
        Establish context = () =>
        {
            questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(
                    new NumericQuestion() { StataExportCaption = "nq1", QuestionType = QuestionType.Numeric, PublicKey = Guid.NewGuid() },
                    new TextQuestion() { StataExportCaption = "tq1", QuestionType = QuestionType.Text, PublicKey = Guid.NewGuid() },
                    new Group("Roster Group")
                    {
                        IsRoster = true,
                        RosterSizeSource = RosterSizeSourceType.FixedTitles,
                        PublicKey = rosterGroupId,
                        RosterFixedTitles = new []{"a"},
                        VariableName = "Roster Group",
                        Children = new List<IComposite> { new NumericQuestion() { StataExportCaption = "nq2", QuestionType = QuestionType.Numeric, PublicKey = Guid.NewGuid() } }
                    });

            preloadedDataService = CreatePreloadedDataService(questionnaireDocument);
        };

        Because of =
            () =>
                result =
                    preloadedDataService.CreatePreloadedDataDtosFromPanelData(new[]
                    {
                        CreatePreloadedDataByFile(new[] { "Id", "nq1" }, new[] { new[] { "1", "2" } }, questionnaireDocument.Title),
                        CreatePreloadedDataByFile(new[] { "Id", "nq2", "ParentId1" }, new[] { new[] { "1", "2", "1" } }, "Roster Group")
                    });

        It should_return_not_null_result = () =>
            result.ShouldNotBeNull();

        It should_result_has_1_items = () =>
           result.Length.ShouldEqual(1);

        private static PreloadedDataService preloadedDataService;
        private static QuestionnaireDocument questionnaireDocument;
        private static PreloadedDataRecord[] result;
        private static Guid rosterGroupId = Guid.NewGuid();
    }
}
