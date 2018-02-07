using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
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
            preloadedDataByFile = CreatePreloadedDataByFile(new[] { ServiceColumns.InterviewId, "num"},
                new string[][] { new string[] { "1", "3,22" } },
                "questionnaire.csv");
            
            preloadedDataService = Create.Service.PreloadedDataService(questionnaire);

            importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataService);
        };

        Because of =
            () => VerificationErrors = importDataVerifier.VerifyPanelFiles(Create.Entity.PreloadedDataByFile(preloadedDataByFile), preloadedDataService).ToList();

        It should_result_has_1_errors = () =>
            VerificationErrors.Count().ShouldEqual(1);

        It should_return_single_PL0030_error = () =>
            VerificationErrors.First().Code.ShouldEqual("PL0034");

        It should_return_single_error_with_explanation_in_message = () =>
            VerificationErrors.First().Message.ToLower().ToSeparateWords().ShouldContain("symbol", "not", "allowed", "numeric", "answers", "please", "use", "decimal", "separator");

        It should_return_error_with_single_reference = () =>
            VerificationErrors.First().References.Count().ShouldEqual(1);

        It should_return_error_with_single_reference_of_type_Cell = () =>
            VerificationErrors.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Cell);

        It should_return_error_with_single_reference_pointing_on_second_column = () =>
            VerificationErrors.First().References.First().PositionX.ShouldEqual(1);

        It should_return_error_with_single_reference_pointing_on_first_row = () =>
            VerificationErrors.First().References.First().PositionY.ShouldEqual(0);


        private static ImportDataVerifier importDataVerifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static Guid numericQuestionId;
        private static PreloadedDataByFile preloadedDataByFile;
        private static ImportDataParsingService preloadedDataService;
    }
}