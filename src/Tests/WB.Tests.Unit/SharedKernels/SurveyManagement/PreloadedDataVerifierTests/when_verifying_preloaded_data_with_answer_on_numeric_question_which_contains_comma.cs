using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Tests.Abc;

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

            importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataService);
        };

        Because of =
            () => importDataVerifier.VerifyPanelFiles(questionnaireId, 1, new[] { preloadedDataByFile }, status);

        It should_result_has_1_errors = () =>
            status.VerificationState.Errors.Count().ShouldEqual(1);

        It should_return_single_PL0030_error = () =>
            status.VerificationState.Errors.First().Code.ShouldEqual("PL0034");

        It should_return_single_error_with_explanation_in_message = () =>
            status.VerificationState.Errors.First().Message.ToLower().ToSeparateWords().ShouldContain("symbol", "not", "allowed", "numeric", "answers", "please", "use", "decimal", "separator");

        It should_return_error_with_single_reference = () =>
            status.VerificationState.Errors.First().References.Count().ShouldEqual(1);

        It should_return_error_with_single_reference_of_type_Cell = () =>
            status.VerificationState.Errors.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Cell);

        It should_return_error_with_single_reference_pointing_on_second_column = () =>
            status.VerificationState.Errors.First().References.First().PositionX.ShouldEqual(1);

        It should_return_error_with_single_reference_pointing_on_first_row = () =>
            status.VerificationState.Errors.First().References.First().PositionY.ShouldEqual(0);


        private static ImportDataVerifier importDataVerifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static Guid numericQuestionId;
        private static PreloadedDataByFile preloadedDataByFile;
    }
}