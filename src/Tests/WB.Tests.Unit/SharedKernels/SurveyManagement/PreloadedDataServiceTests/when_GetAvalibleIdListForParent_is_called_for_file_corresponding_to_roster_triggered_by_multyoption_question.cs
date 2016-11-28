using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    internal class when_GetAvalibleIdListForParent_is_called_for_file_corresponding_to_roster_triggered_by_multyoption_question :
        PreloadedDataServiceTestContext
    {
        private Establish context = () =>
        {
            questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(
                    new MultyOptionsQuestion()
                    {
                        PublicKey = rosterSizeQuestionId,
                        QuestionType = QuestionType.MultyOption,
                        StataExportCaption = rosterSizeQuestionVariableName,
                        Answers = new List<Answer> { new Answer() { AnswerValue = "3", AnswerText = "three" } }
                    },
                    new Group("Roster Group")
                    {
                        IsRoster = true,
                        RosterSizeSource = RosterSizeSourceType.Question,
                        PublicKey = rosterGroupId,
                        RosterSizeQuestionId = rosterSizeQuestionId
                    });

            preloadedDataService = CreatePreloadedDataService(questionnaireDocument);
        };

        private Because of =
            () =>
                result =
                    preloadedDataService.GetAvailableIdListForParent(
                        CreatePreloadedDataByFile(
                            new string[] { "Id", rosterSizeQuestionVariableName + "_0", rosterSizeQuestionVariableName + "_1" },
                            new string[][] { new string[] { "1", "3", "" } },
                            questionnaireDocument.Title),new ValueVector<Guid> { rosterSizeQuestionId}, new []{"1"}, new PreloadedDataByFile[0]);

        private It should_return_not_null_result = () =>
            result.ShouldNotBeNull();

        private It should_result_have_2_ids_1_and_2 = () =>
            result.SequenceEqual(new decimal[] { 1 });

        private static PreloadedDataService preloadedDataService;
        private static QuestionnaireDocument questionnaireDocument;
        private static decimal[] result;
        private static Guid rosterGroupId = Guid.NewGuid();
        private static Guid rosterSizeQuestionId = Guid.NewGuid();
        private static string rosterSizeQuestionVariableName = "var";
    }
}
