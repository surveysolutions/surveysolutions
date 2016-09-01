using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_with_answer_on_numeric_question_which_contains_comma : PreloadedDataVerifierTestContext
    {
        private Establish context = () =>
        {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            numericQuestionId = Guid.Parse("21111111111111111111111111111111");
            var gpsQuestion = Create.Entity.NumericQuestion(questionId: numericQuestionId, variableName: "num");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(gpsQuestion);
            questionnaire.Title = "questionnaire";
            preloadedDataByFile = CreatePreloadedDataByFile(new[] { "Id", "num"},
                new string[][] { new string[] { "1", "3,22" } },
                "questionnaire.csv");

            var preloadedDataService =
                Create.Service.PreloadedDataService(questionnaire);

            preloadedDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataService);
        };

        Because of =
            () => result = preloadedDataVerifier.VerifyPanel(questionnaireId, 1, new[] { preloadedDataByFile });

        It should_result_has_1_errors = () =>
            result.Errors.Count().ShouldEqual(1);

        It should_return_single_PL0030_error = () =>
            result.Errors.First().Code.ShouldEqual("PL0034");

        It should_return_single_error_with_explanation_in_message = () =>
           result.Errors.First().Message.ToLower().ToSeparateWords().ShouldContain("symbol", "not", "allowed", "numeric", "answers", "please", "use", "decimal", "separator");

        It should_return_error_with_single_reference = () =>
            result.Errors.First().References.Count().ShouldEqual(1);

        It should_return_error_with_single_reference_of_type_Cell = () =>
            result.Errors.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Cell);

        It should_return_error_with_single_reference_pointing_on_second_column = () =>
           result.Errors.First().References.First().PositionX.ShouldEqual(1);

        It should_return_error_with_single_reference_pointing_on_first_row = () =>
            result.Errors.First().References.First().PositionY.ShouldEqual(0);


        private static PreloadedDataVerifier preloadedDataVerifier;
        private static VerificationStatus result;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static Guid numericQuestionId;
        private static PreloadedDataByFile preloadedDataByFile;
    }
}