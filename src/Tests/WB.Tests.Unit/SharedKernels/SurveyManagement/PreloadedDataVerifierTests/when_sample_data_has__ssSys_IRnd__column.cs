using System;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_sample_data_has__ssSys_IRnd__column : PreloadedDataVerifierTestContext
    {
        [Test]
        public void should_ignore_column()
        {
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var numericQuestionId = Guid.Parse("21111111111111111111111111111111");
            var question = Create.Entity.NumericQuestion(questionId: numericQuestionId, variableName: "num");

            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(question);
            questionnaire.Title = "questionnaire";
            var preloadedDataByFile = CreatePreloadedDataByFile(new[] { "Id", "num", "ssSys_IRnd" },
                new[] { new string[] { "1", "5", "3.22" } },
                "questionnaire.csv");

            var preloadedDataService = Create.Service.PreloadedDataService(questionnaire);

            var preloadedDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataService);

            preloadedDataVerifier.VerifyPanelFiles(questionnaireId, 1, new[] { preloadedDataByFile }, status);

            Assert.That(status.VerificationState.Errors, Is.Empty);
        }
    }
}