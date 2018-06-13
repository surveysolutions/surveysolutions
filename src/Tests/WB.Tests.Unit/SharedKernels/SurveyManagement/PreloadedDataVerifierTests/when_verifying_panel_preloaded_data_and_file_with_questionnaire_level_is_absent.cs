using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    [TestFixture]
    internal class when_verifying_panel_preloaded_data_and_file_with_questionnaire_level_is_absent : PreloadedDataVerifierTestContext
    {
        [Test]
        public void should_return_error_with_code_PL0040()
        {
            var questionnaireId = Guid.NewGuid();
            var questionnaireDocument = Create.Entity.QuestionnaireDocument(questionnaireId);
            questionnaireDocument.Title = "title";

            var importDataVerifier = CreatePreloadedDataVerifier(questionnaireDocument);
            var assignmentData = Create.Entity.PreloadedDataByFile(
                Create.Entity.PreloadedDataByFile(fileName: "roster1"),
                Create.Entity.PreloadedDataByFile(fileName: "roster2")
            );

            importDataVerifier.VerifyPanelFiles(Guid.NewGuid(), 1, assignmentData, status);

            status.VerificationState.Errors.Count().Should().Be(1);
            status.VerificationState.Errors.First().Code.Should().Be("PL0040");
        }
    }
}
