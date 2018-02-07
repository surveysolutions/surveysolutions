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
    internal class when_verifying_preloaded_data_with_column_duplicates : PreloadedDataVerifierTestContext
    {
        private Establish context = () =>
        {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            numericQuestionId = Guid.Parse("21111111111111111111111111111111");
            var gpsQuestion = Create.Entity.NumericIntegerQuestion(numericQuestionId, "num");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(gpsQuestion);
            questionnaire.Title = "questionnaire";
            preloadedDataByFile = CreatePreloadedDataByFile(new[] { ServiceColumns.InterviewId , "num", "num" },
                new string[][] { new string[] { "1", "3", "3" } },
                "questionnaire.csv");
            
            preloadedDataService = Create.Service.PreloadedDataService(questionnaire);

            importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataService);
        };

        Because of =
            () => VerificationErrors = importDataVerifier.VerifyPanelFiles(Create.Entity.PreloadedDataByFile(preloadedDataByFile), preloadedDataService).ToList();

        It should_result_has_1_errors = () =>
            VerificationErrors.Count().ShouldEqual(1);

        It should_return_single_PL0031_error = () =>
            VerificationErrors.First().Code.ShouldEqual("PL0031");

        It should_return_error_with_two_references = () =>
            VerificationErrors.First().References.Count().ShouldEqual(2);

        It should_return_error_with_first_reference_of_type_Column = () =>
            VerificationErrors.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Column);

        It should_return_error_with_first_reference_null_pointing = () =>
            VerificationErrors.First().References.First().PositionX.ShouldBeNull();

        It should_return_error_with_first_reference_pointing_on_second_column = () =>
            VerificationErrors.First().References.First().PositionY.ShouldEqual(1);

        It should_return_error_with_second_reference_of_type_Column = () =>
            VerificationErrors.First().References.Last().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Column);

        It should_return_error_with_second_reference_null_pointing = () =>
            VerificationErrors.First().References.Last().PositionX.ShouldBeNull();

        It should_return_error_with_second_reference_pointing_on_second_column = () =>
            VerificationErrors.First().References.Last().PositionY.ShouldEqual(2);


        private static ImportDataVerifier importDataVerifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static Guid numericQuestionId;
        private static PreloadedDataByFile preloadedDataByFile;
        private static ImportDataParsingService preloadedDataService;
    }
}