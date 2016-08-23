using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;

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
            preloadedDataByFile = CreatePreloadedDataByFile(new[] { "Id", "gps__Latitude", "gps__Longitude", "gps__Timestamp" },
                new string[][] { new string[] { "1", "3", "3", "1" } },
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
            result.Errors.First().Code.ShouldEqual("PL0017");

        It should_return_error_with_single_reference = () =>
            result.Errors.First().References.Count().ShouldEqual(1);

        It should_return_error_with_single_reference_of_type_Cell = () =>
            result.Errors.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Cell);

        It should_return_error_with_single_reference_pointing_on_fourth_column = () =>
           result.Errors.First().References.First().PositionX.ShouldEqual(3);

        It should_return_error_with_single_reference_pointing_on_second_row = () =>
            result.Errors.First().References.First().PositionY.ShouldEqual(0);


        private static PreloadedDataVerifier preloadedDataVerifier;
        private static VerificationStatus result;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static Guid gpsQuestionId;
        private static PreloadedDataByFile preloadedDataByFile;
    }
}