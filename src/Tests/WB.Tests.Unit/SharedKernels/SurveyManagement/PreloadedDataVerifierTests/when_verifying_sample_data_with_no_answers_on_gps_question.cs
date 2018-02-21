using System;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_sample_data_with_no_answers_on_gps_question : PreloadedDataVerifierTestContext
    {
        [Test]
        public void should_return_no_errors()
        {
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var gpsQuestionId = Guid.Parse("21111111111111111111111111111111");
            var gpsQuestion = Create.Entity.GpsCoordinateQuestion(gpsQuestionId, "gps");

            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(gpsQuestion);
            questionnaire.Title = "questionnaire";
            var preloadedDataByFile = CreatePreloadedDataByFile(new[] { ServiceColumns.InterviewId },
                new[] { new[] { "1" } },
                "questionnaire.tab");

            var preloadedDataService =
                Create.Service.PreloadedDataService(questionnaire);

            var importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataService);
            var result = importDataVerifier.VerifyAssignmentsSample(preloadedDataByFile, preloadedDataService);

            Assert.That(result, Is.Empty);
        }
    }
}