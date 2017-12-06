using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_uploaded_quantity_is_minus_2 : PreloadedDataVerifierTestContext
    {
        [Test]
        public void should_return_pl_error()
        {
            var questionnaire = CreateQuestionnaireDocumentWithOneChapter();
            var QuestionnaireCsvFileName = "questionnaire.csv";
            var preloadedDataByFile = CreatePreloadedDataByFile(new[] { "_quantity" }, new string[][] { new string[] { "-2" } },
                QuestionnaireCsvFileName);

            var preloadedDataServiceMock = new Mock<IPreloadedDataService>();
            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(QuestionnaireCsvFileName)).Returns(new HeaderStructureForLevel());
            preloadedDataServiceMock.Setup(x => x.GetColumnIndexByHeaderName(It.IsAny<PreloadedDataByFile>(), "_responsible")).Returns(-1);
            preloadedDataServiceMock.Setup(x => x.GetColumnIndexByHeaderName(It.IsAny<PreloadedDataByFile>(), "_quantity")).Returns(0);

            var importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataServiceMock.Object);

            // Act
            var result = importDataVerifier.VerifyAssignmentsSample(Id.g1, 1, preloadedDataByFile);

            // Assert
            result.Errors.Should().Contain(x => x.Code == "PL0036");
        }
    }
}