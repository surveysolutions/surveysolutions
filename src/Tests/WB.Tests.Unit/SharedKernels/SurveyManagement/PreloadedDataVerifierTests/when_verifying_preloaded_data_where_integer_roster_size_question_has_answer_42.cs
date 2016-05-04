using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_where_integer_roster_size_question_has_answer_42 : PreloadedDataVerifierTestContext
    {
        private Establish context = () =>
        {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            numericQuestionId = Guid.Parse("21111111111111111111111111111111");
            var numericQuestion = Create.NumericIntegerQuestion(numericQuestionId, "num");

            questionnaire =
                CreateQuestionnaireDocumentWithOneChapter(chapterChildren:
                    new IComposite[]
                    {
                        numericQuestion,
                        Create.Roster(rosterSizeQuestionId: numericQuestionId,
                            rosterSizeSourceType: RosterSizeSourceType.Question)
                    });

            questionnaire.Title = "questionnaire";
            preloadedDataByFile = CreatePreloadedDataByFile(new[] { "Id", "num" },
                new string[][] { new string[] { "1", "42" } },
                "questionnaire.csv");

            var preloadedDataService =
                Create.PreloadedDataService(questionnaire);

            preloadedDataVerifier = CreatePreloadedDataVerifier(questionnaire, new QuestionDataParser(), preloadedDataService);
        };

        Because of =
            () =>
                result = preloadedDataVerifier.VerifyPanel(questionnaireId, 1, new[] { preloadedDataByFile });

        private It should_result_has_1_error = () =>
               result.Errors.Count().ShouldEqual(1);

        private It should_return_single_PL0029_error = () =>
            result.Errors.First().Code.ShouldEqual("PL0029");

        private It should_return_reference_with_Cell_type = () =>
            result.Errors.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Cell);

        private static PreloadedDataVerifier preloadedDataVerifier;
        private static VerificationStatus result;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static Guid numericQuestionId;
        private static PreloadedDataByFile preloadedDataByFile;
    }
}