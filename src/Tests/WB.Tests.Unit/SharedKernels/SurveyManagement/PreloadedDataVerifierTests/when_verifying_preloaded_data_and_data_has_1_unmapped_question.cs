using System;
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
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var preloadedDataByFile = CreatePreloadedDataByFile(new[] {ServiceColumns.InterviewId, "q1"}, null,
                QuestionnaireCsvFileName);

            var preloadedDataServiceMock = new Mock<IPreloadedDataService>();
            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(QuestionnaireCsvFileName))
                .Returns(new HeaderStructureForLevel(){LevelIdColumnName = ServiceColumns.InterviewId });
            var importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataServiceMock.Object);

            //act
            importDataVerifier.VerifyPanelFiles(questionnaireId, 1, Create.Entity.PreloadedData(preloadedDataByFile), status);

            Assert.AreEqual(status.VerificationState.Errors.Count(), 1);
            Assert.AreEqual(status.VerificationState.Errors.First().Code, "PL0003");
            Assert.AreEqual(status.VerificationState.Errors.First().References.First().Type,
                PreloadedDataVerificationReferenceType.Column);
        }
    }
}
