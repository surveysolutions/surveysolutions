using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_with_column_duplicates : PreloadedDataVerifierTestContext
    {
        [Test]
        public void should_return_single_error()
        {
            var questionnaireId = Id.g1;
            var numericQuestionId = Id.g2;
            var numericIntegerQuestion = Create.Entity.NumericIntegerQuestion(numericQuestionId, "num");

            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(numericIntegerQuestion);
            questionnaire.Title = "questionnaire";
            var preloadedDataByFile = CreatePreloadedDataByFile(new[] { ServiceColumns.InterviewId, "num", "num" },
                new[] { new[] { "1", "3", "3" } },
                "questionnaire.csv");

            var preloadedDataService = Create.Service.PreloadedDataService(questionnaire);

            var importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataService);

            // Act
            importDataVerifier.VerifyPanelFiles(questionnaireId, 1, Create.Entity.PreloadedDataByFile(preloadedDataByFile), status);

            // Assert
            Assert.That(status.VerificationState.Errors, Has.Count.EqualTo(1));

            var panelImportVerificationError = status.VerificationState.Errors.First();

            panelImportVerificationError.Code.Should().Be("PL0031");
            
            panelImportVerificationError.References.Should().HaveCount(2);

            var interviewImportReference = panelImportVerificationError.References.First();
            interviewImportReference.Type.Should().Be(PreloadedDataVerificationReferenceType.Column);
            interviewImportReference.PositionX.Should().BeNull();
            interviewImportReference.PositionY.Should().Be(1);
            panelImportVerificationError.References.Last().Type.Should().Be(PreloadedDataVerificationReferenceType.Column);
            panelImportVerificationError.References.Last().PositionX.Should().BeNull();
            panelImportVerificationError.References.Last().PositionY.Should().Be(2);
        }

    }
}
