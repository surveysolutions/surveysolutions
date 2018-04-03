using FluentAssertions;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    internal class when_GetParentDataFile_is_called_for_first_level_of_questionnaire : PreloadedDataServiceTestContext
    {
        [Test]
        public void should_result_filename_be_equal_to_top_level_file()
        {
            var questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                Create.Entity.FixedRoster(rosterId: Id.g1, title: "Roster Group", variable: "roster", obsoleteFixedTitles: new[] { "1" })
            );

            var importDataParsingService = CreatePreloadedDataService(questionnaireDocument);

            // Act
            var result = importDataParsingService.GetParentDataFile("roster", Create.Entity.PreloadedDataByFile(
                CreatePreloadedDataByFile(null, null, "roster"),
                CreatePreloadedDataByFile(null, null, questionnaireDocument.Title)));

            // Assert
            result.FileName.Should().Be(questionnaireDocument.Title);
        }
    }
}
