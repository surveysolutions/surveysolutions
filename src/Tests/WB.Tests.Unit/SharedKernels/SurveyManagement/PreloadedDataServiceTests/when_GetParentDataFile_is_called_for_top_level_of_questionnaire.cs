using FluentAssertions;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    internal class when_GetParentDataFile_is_called_for_top_level_of_questionnaire : PreloadedDataServiceTestContext
    {
        [Test]
        public void should_result_be_null()
        {
            var questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(
                    Create.Entity.FixedRoster(rosterId: Id.g1,
                        obsoleteFixedTitles: new[] { "1" }, title: "Roster Group"));

            var importDataParsingService = CreatePreloadedDataService(questionnaireDocument);

            // Act
            var result =
                importDataParsingService.GetParentDataFile(questionnaireDocument.Title, 
                    Create.Entity.PreloadedDataByFile(CreatePreloadedDataByFile(null, null, "Roster Group"), 
                        CreatePreloadedDataByFile(null, null, questionnaireDocument.Title)));

            // Assert
            result.Should().BeNull();
        }
    }
}
