using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.PreloadedDataServiceTests
{
    internal class when_GetAvalibleIdListForParent_is_called_for_file_corresponding_to_roster_triggered_by_numeric_question : PreloadedDataServiceTestContext
    {
        Establish context = () =>
        {
            questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(
                    new NumericQuestion() { PublicKey = rosterSizeQuestionId, QuestionType = QuestionType.Numeric, StataExportCaption = rosterSizeQuestionVariableName, IsInteger = true, MaxValue = 2 },
                    new Group("Roster Group")
                    {
                        IsRoster = true,
                        RosterSizeSource = RosterSizeSourceType.Question,
                        PublicKey = rosterGroupId,
                        RosterSizeQuestionId = rosterSizeQuestionId
                    });

            preloadedDataService = CreatePreloadedDataService(questionnaireDocument);
        };

        Because of =
            () =>
                result =
                    preloadedDataService.GetAvailableIdListForParent(
                        CreatePreloadedDataByFile(new string[] { "Id", rosterSizeQuestionVariableName }, new string[][] { new string[] { "1","3" } },
                            questionnaireDocument.Title), new ValueVector<Guid> { rosterSizeQuestionId }, new []{"1"});

        It should_return_not_null_result = () =>
            result.ShouldNotBeNull();

        It should_result_have_2_ids_1_and_2 = () =>
            result.SequenceEqual(new decimal[] { 0, 1 }).ShouldBeTrue();

        private static PreloadedDataService preloadedDataService;
        private static QuestionnaireDocument questionnaireDocument;
        private static decimal[] result;
        private static Guid rosterGroupId = Guid.NewGuid();
        private static Guid rosterSizeQuestionId = Guid.NewGuid();
        private static string rosterSizeQuestionVariableName = "var";
    }
}
