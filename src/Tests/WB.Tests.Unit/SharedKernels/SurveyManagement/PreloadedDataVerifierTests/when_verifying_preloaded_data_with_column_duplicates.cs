using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_with_column_duplicates : PreloadedDataVerifierTestContext
    {
        private Establish context = () =>
        {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            numericQuestionId = Guid.Parse("21111111111111111111111111111111");
            var gpsQuestion = Create.Entity.NumericIntegerQuestion(numericQuestionId, "num");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(gpsQuestion);
            questionnaire.Title = "questionnaire";
            preloadedDataByFile = CreatePreloadedDataByFile(new[] { "Id", "num", "num" },
                new string[][] { new string[] { "1", "3", "3" } },
                "questionnaire.csv");

            var preloadedDataService = Create.Service.PreloadedDataService(questionnaire);

            preloadedDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataService);
        };

        Because of =
            () => result = preloadedDataVerifier.VerifyPanel(questionnaireId, 1, new[] { preloadedDataByFile });

        It should_result_has_1_errors = () =>
            result.Errors.Count().ShouldEqual(1);

        It should_return_single_PL0031_error = () =>
            result.Errors.First().Code.ShouldEqual("PL0031");

        It should_return_error_with_two_references = () =>
            result.Errors.First().References.Count().ShouldEqual(2);

        It should_return_error_with_first_reference_of_type_Column = () =>
            result.Errors.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Column);

        It should_return_error_with_first_reference_null_pointing = () =>
           result.Errors.First().References.First().PositionX.ShouldBeNull();

        It should_return_error_with_first_reference_pointing_on_second_column = () =>
         result.Errors.First().References.First().PositionY.ShouldEqual(1);

        It should_return_error_with_second_reference_of_type_Column = () =>
           result.Errors.First().References.Last().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Column);

        It should_return_error_with_second_reference_null_pointing = () =>
           result.Errors.First().References.Last().PositionX.ShouldBeNull();

        It should_return_error_with_second_reference_pointing_on_second_column = () =>
         result.Errors.First().References.Last().PositionY.ShouldEqual(2);


        private static PreloadedDataVerifier preloadedDataVerifier;
        private static VerificationStatus result;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static Guid numericQuestionId;
        private static PreloadedDataByFile preloadedDataByFile;
    }
}