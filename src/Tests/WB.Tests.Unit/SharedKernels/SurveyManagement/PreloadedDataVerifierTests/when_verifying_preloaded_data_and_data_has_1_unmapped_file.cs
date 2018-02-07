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
            var  questionnaire = CreateQuestionnaireDocumentWithOneChapter();
            var preloadedDataServiceMock = new Mock<IPreloadedDataService>();

            var importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataServiceMock.Object);

            VerificationErrors = importDataVerifier.VerifyPanelFiles(
                Create.Entity.PreloadedDataByFile(CreatePreloadedDataByFile(fileName: questionnaire.Title + ".csv")),
                preloadedDataServiceMock.Object).ToList();

            Assert.AreEqual(VerificationErrors.Count(), 1);
            Assert.AreEqual(VerificationErrors.First().Code,"PL0004");
            Assert.AreEqual(VerificationErrors.First().References.First().Type, PreloadedDataVerificationReferenceType.File);

     }
}
}
