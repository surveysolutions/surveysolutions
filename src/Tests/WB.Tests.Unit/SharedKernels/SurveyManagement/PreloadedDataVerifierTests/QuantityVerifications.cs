using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class QuantityVerifications : PreloadedDataVerifierTestContext
    {
        [Test]
        public void when_panel_data_has_no_quantity_column__Should_not_verify_it()
        {
            var questionnaireId = Id.g1;
            var numericQuestionId = Id.g2;
            var numericIntegerQuestion = Create.Entity.NumericIntegerQuestion(numericQuestionId, "num");

            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(numericIntegerQuestion);
            questionnaire.Title = "questionnaire";
            var preloadedDataByFile = CreatePreloadedDataByFile(new[] { ServiceColumns.InterviewId, "num"},
                new[] { new[] { "1", "3"} },
                "questionnaire.csv");

            var preloadedDataService = Create.Service.PreloadedDataService(questionnaire);

            var importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataService);

            // Act
            importDataVerifier.VerifyPanelFiles(questionnaireId, 1, Create.Entity.PreloadedDataByFile(preloadedDataByFile), status);

            // Assert
            Assert.That(status.VerificationState.Errors, Has.Count.EqualTo(0));
        }

        [Test]
        public void when_panel_data_has_invalid_text_in_quantity_column__Should_return_error()
        {
            var questionnaireId = Id.g1;
            var numericQuestionId = Id.g2;
            var numericIntegerQuestion = Create.Entity.NumericIntegerQuestion(numericQuestionId, "num");

            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(numericIntegerQuestion);
            questionnaire.Title = "questionnaire";
            var preloadedDataByFile = CreatePreloadedDataByFile(new[] { ServiceColumns.InterviewId, "num", ServiceColumns.AssignmentsCountColumnName},
                new[] { new[] { "1", "3", "asb"} },
                "questionnaire.csv");

            var preloadedDataService = Create.Service.PreloadedDataService(questionnaire);

            var importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataService);

            // Act
            importDataVerifier.VerifyPanelFiles(questionnaireId, 1, Create.Entity.PreloadedDataByFile(preloadedDataByFile), status);

            // Assert
            Assert.That(status.VerificationState.Errors, Has.Count.EqualTo(1));
            Assert.That(status.VerificationState.Errors.First(), Has.Property(nameof(PanelImportVerificationError.Code)).EqualTo("PL0035"));
        }
    }
}
