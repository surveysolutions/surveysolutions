using System.Linq;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_and_data_has_1_unmapped_question : PreloadedDataVerifierTestContext
    {
        [Test]
        public void Should_result_has_1_error()
        {
            var QuestionnaireCsvFileName = "questionnaire.csv";
            var questionnaire = CreateQuestionnaireDocumentWithOneChapter();
            questionnaire.Title = "questionnaire";
            var preloadedDataByFile = CreatePreloadedDataByFile(new[] {ServiceColumns.InterviewId, "q1"}, null,
                QuestionnaireCsvFileName);

            var preloadedDataServiceMock = new Mock<IPreloadedDataService>();
            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(QuestionnaireCsvFileName))
                .Returns(new HeaderStructureForLevel(){LevelIdColumnName = ServiceColumns.InterviewId });
            var importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataServiceMock.Object);

            //act
            VerificationErrors = importDataVerifier.VerifyPanelFiles(Create.Entity.PreloadedDataByFile(preloadedDataByFile), preloadedDataServiceMock.Object).ToList();

            Assert.AreEqual(VerificationErrors.Count(), 1);
            Assert.AreEqual(VerificationErrors.First().Code, "PL0003");
            Assert.AreEqual(VerificationErrors.First().References.First().Type,
                PreloadedDataVerificationReferenceType.Column);
        }
    }
}
