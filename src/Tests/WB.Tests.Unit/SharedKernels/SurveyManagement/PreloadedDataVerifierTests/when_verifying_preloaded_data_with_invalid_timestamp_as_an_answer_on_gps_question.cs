using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_with_invalid_timestamp_as_an_answer_on_gps_question : PreloadedDataVerifierTestContext
    {
        private Establish context = () =>
        {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            gpsQuestionId = Guid.Parse("21111111111111111111111111111111");
            var gpsQuestion = Create.Entity.GpsCoordinateQuestion(gpsQuestionId, "gps");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(gpsQuestion);
            questionnaire.Title = "questionnaire";
            preloadedDataByFile = CreatePreloadedDataByFile(new[] { ServiceColumns.InterviewId, "gps__Latitude", "gps__Longitude", "gps__Timestamp" },
                new string[][] { new string[] { "1", "3", "3", "1" } },
                "questionnaire.csv");
            
            preloadedDataService = Create.Service.PreloadedDataService(questionnaire);

            importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataService);
        };

        Because of =
            () => VerificationErrors = importDataVerifier.VerifyPanelFiles(Create.Entity.PreloadedDataByFile(preloadedDataByFile), preloadedDataService).ToList();

        It should_result_has_1_errors = () =>
            VerificationErrors.Count().ShouldEqual(1);

        It should_return_single_PL0030_error = () =>
            VerificationErrors.First().Code.ShouldEqual("PL0017");

        It should_return_error_with_single_reference = () =>
            VerificationErrors.First().References.Count().ShouldEqual(1);

        It should_return_error_with_single_reference_of_type_Cell = () =>
            VerificationErrors.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Cell);

        It should_return_error_with_single_reference_pointing_on_fourth_column = () =>
            VerificationErrors.First().References.First().PositionX.ShouldEqual(3);

        It should_return_error_with_single_reference_pointing_on_second_row = () =>
            VerificationErrors.First().References.First().PositionY.ShouldEqual(0);


        private static ImportDataVerifier importDataVerifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static Guid gpsQuestionId;
        private static PreloadedDataByFile preloadedDataByFile;
        private static ImportDataParsingService preloadedDataService;
    }
}