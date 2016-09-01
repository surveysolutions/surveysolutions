using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_with_invalid_longitude_as_an_answer_on_gps_question : PreloadedDataVerifierTestContext
    {
        private Establish context = () =>
        {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            gpsQuestionId = Guid.Parse("21111111111111111111111111111111");
            var gpsQuestion = Create.Entity.GpsCoordinateQuestion(gpsQuestionId, "gps");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(gpsQuestion);
            questionnaire.Title = "questionnaire";
            preloadedDataByFile = CreatePreloadedDataByFile(new[] { "Id", "gps__Latitude", "gps__Longitude" },
                new string[][] { new string[] { "1", "3", "180.00001" }, new string[] { "2", "3", "-180.00001" } },
                "questionnaire.csv");

            var preloadedDataService =
                Create.Service.PreloadedDataService(questionnaire);

            preloadedDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataService);
        };

        Because of =
            () => result = preloadedDataVerifier.VerifyPanel(questionnaireId, 1, new[] { preloadedDataByFile });

        It should_result_has_2_errors = () =>
            result.Errors.Count().ShouldEqual(2);

        It should_return_first_PL0030_error = () =>
            result.Errors.First().Code.ShouldEqual("PL0033");

        It should_return_first_error_with_single_reference = () =>
            result.Errors.First().References.Count().ShouldEqual(1);

        It should_return_first_error_with_single_reference_of_type_Cell = () =>
            result.Errors.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Cell);

        It should_return_first_error_with_single_reference_pointing_on_first_row = () =>
            result.Errors.First().References.First().PositionY.ShouldEqual(0);

        It should_return_first_error_with_single_reference_pointing_on_third_column = () =>
            result.Errors.First().References.First().PositionX.ShouldEqual(2);

        It should_return_second_PL0030_error = () =>
            result.Errors.Second().Code.ShouldEqual("PL0033");

        It should_return_second_error_with_single_reference = () =>
            result.Errors.Second().References.Count().ShouldEqual(1);

        It should_return_second_error_with_single_reference_of_type_Cell = () =>
            result.Errors.Second().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Cell);

        It should_return_second_error_with_single_reference_pointing_on_first_row = () =>
            result.Errors.Second().References.First().PositionY.ShouldEqual(1);

        It should_return_second_error_with_single_reference_pointing_on_third_column = () =>
            result.Errors.Second().References.First().PositionX.ShouldEqual(2);


        private static PreloadedDataVerifier preloadedDataVerifier;
        private static VerificationStatus result;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static Guid gpsQuestionId;
        private static PreloadedDataByFile preloadedDataByFile;
    }
}