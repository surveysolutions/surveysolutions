using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_with_invalid_latitude_as_an_answer_on_gps_question : PreloadedDataVerifierTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            gpsQuestionId = Guid.Parse("21111111111111111111111111111111");
            var gpsQuestion = Create.Entity.GpsCoordinateQuestion(gpsQuestionId, "gps");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(gpsQuestion);
            questionnaire.Title = "questionnaire";
            preloadedDataByFile = CreatePreloadedDataByFile(new[] { ServiceColumns.InterviewId, "gps__Latitude", "gps__Longitude" },
                new[] { new[] { "1", "90.00001", "3" }, new[] { "1", "-90.00001", "3" } },
                "questionnaire.csv");

            var preloadedDataService =
                Create.Service.PreloadedDataService(questionnaire);

            importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataService);
            BecauseOf();
        }

        private void BecauseOf() => importDataVerifier.VerifyPanelFiles(questionnaireId, 1, Create.Entity.PreloadedDataByFile(preloadedDataByFile), status);

        [NUnit.Framework.Test] public void should_result_has_2_errors () =>
            status.VerificationState.Errors.Count().Should().Be(2);

        [NUnit.Framework.Test] public void should_return_first_PL0030_error () =>
            status.VerificationState.Errors.First().Code.Should().Be("PL0032");

        [NUnit.Framework.Test] public void should_return_first_error_with_single_reference () =>
            status.VerificationState.Errors.First().References.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_first_error_with_single_reference_of_type_Cell () =>
            status.VerificationState.Errors.First().References.First().Type.Should().Be(PreloadedDataVerificationReferenceType.Cell);

        [NUnit.Framework.Test] public void should_return_first_error_with_single_reference_pointing_on_first_column () =>
            status.VerificationState.Errors.First().References.First().PositionX.Should().Be(1);

        [NUnit.Framework.Test] public void should_return_first_error_with_single_reference_pointing_on_second_row () =>
            status.VerificationState.Errors.First().References.First().PositionY.Should().Be(0);

        [NUnit.Framework.Test] public void should_return_second_PL0030_error () =>
            status.VerificationState.Errors.Second().Code.Should().Be("PL0032");

        [NUnit.Framework.Test] public void should_return_second_error_with_single_reference () =>
            status.VerificationState.Errors.Second().References.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_second_error_with_single_reference_of_type_Cell () =>
            status.VerificationState.Errors.Second().References.First().Type.Should().Be(PreloadedDataVerificationReferenceType.Cell);

        [NUnit.Framework.Test] public void should_return_second_error_with_single_reference_pointing_on_first_column () =>
            status.VerificationState.Errors.Second().References.First().PositionX.Should().Be(1);

        [NUnit.Framework.Test] public void should_return_second_error_with_single_reference_pointing_on_second_row () =>
            status.VerificationState.Errors.Second().References.First().PositionY.Should().Be(1);


        private static ImportDataVerifier importDataVerifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static Guid gpsQuestionId;
        private static PreloadedDataByFile preloadedDataByFile;
    }
}
