using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_sample_preloaded_data_file_has_no_id_and_parent_and_responsible_columns : PreloadedDataVerifierTestContext
    {
        [Test] public void should_result_has_no_errors () {
            var questionnaire = CreateQuestionnaireDocumentWithOneChapter();
            questionnaire.Title = "questionnaire";
            var questionnaireId = Id.g1;
            const string QuestionnaireCsvFileName = "questionnaire.csv";

            var preloadedDataByFile = CreatePreloadedDataByFile(new string[0], new string[][] {new string[0]},
                QuestionnaireCsvFileName);
            var preloadedDataServiceMock = new Mock<IPreloadedDataService>();

            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(QuestionnaireCsvFileName)).Returns(new HeaderStructureForLevel());

            preloadedDataServiceMock.Setup(x => x.GetColumnIndexByHeaderName(Moq.It.IsAny<PreloadedDataByFile>(), "_responsible")).Returns(-1);
            preloadedDataServiceMock.Setup(x => x.GetColumnIndexByHeaderName(Moq.It.IsAny<PreloadedDataByFile>(), "_quantity")).Returns(-1);

            var importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataServiceMock.Object);

            // Act
            var result = importDataVerifier.VerifyAssignmentsSample(questionnaireId, 1, preloadedDataByFile);

            // Assert
            result.Errors.Should().BeEmpty();
        }
    }
}
