using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_and_data_has_1_unmapped_file : PreloadedDataVerifierTestContext
    {
        [Test]
        public void Should_return_1_error()
        {
            var questionnaire = CreateQuestionnaireDocumentWithOneChapter();
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");


            var preloadedDataServiceMock = new Mock<IPreloadedDataService>();

            var importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataServiceMock.Object);

            importDataVerifier.VerifyPanelFiles(questionnaireId,
                    1,
                    Create.Entity.PreloadedDataByFile(CreatePreloadedDataByFile(fileName: questionnaire.Title + ".csv")),
                    status);

            Assert.AreEqual(status.VerificationState.Errors.Count(), 1);
            Assert.AreEqual(status.VerificationState.Errors.First().Code, "PL0004");
            Assert.AreEqual(status.VerificationState.Errors.First().References.First().Type, PreloadedDataVerificationReferenceType.File);

        }
    }
}
