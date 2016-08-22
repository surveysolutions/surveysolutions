using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_where_integer_long_roster_size_question_has_answer_202 : PreloadedDataVerifierTestContext
    {
        private Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(chapterChildren:
                new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(numericQuestionId, "num"),
                    Create.Entity.Roster(rosterSizeQuestionId: numericQuestionId, rosterSizeSourceType: RosterSizeSourceType.Question)
                });

            questionnaire.Title = "questionnaire";
            preloadedDataByFile = CreatePreloadedDataByFile(
                new[] { "Id", "num" },
                new[] { new[] { "1", "202" } },
                "questionnaire.csv");

            var preloadedDataService = Create.Service.PreloadedDataService(questionnaire);

            preloadedDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataService);
        };

        Because of =
            () => result = preloadedDataVerifier.VerifyPanel(questionnaireId, 1, new[] { preloadedDataByFile });

        It should_result_has_1_error = () =>
            result.Errors.Count().ShouldEqual(1);

        It should_return_single_PL0029_error = () =>
            result.Errors.First().Code.ShouldEqual("PL0029");

        It should_return_reference_with_Cell_type = () =>
            result.Errors.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Cell);

        private static PreloadedDataVerifier preloadedDataVerifier;
        private static VerificationStatus result;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid numericQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static PreloadedDataByFile preloadedDataByFile;
    }
}