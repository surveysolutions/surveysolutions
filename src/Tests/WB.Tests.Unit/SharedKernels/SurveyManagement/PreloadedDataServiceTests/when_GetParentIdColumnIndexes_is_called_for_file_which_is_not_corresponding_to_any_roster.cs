using FluentAssertions;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    internal class when_GetParentIdColumnIndexes_is_called_for_file_which_is_not_corresponding_to_any_roster : PreloadedDataServiceTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter();

            importDataParsingService = CreatePreloadedDataService(questionnaireDocument);
            BecauseOf();
        }

        private void BecauseOf() => result = importDataParsingService.GetParentIdColumnIndexes(
                        CreatePreloadedDataByFile(new string[] { "Id" }, new string[][] { new string[] { "1" } },
                            "random file name"));

        [NUnit.Framework.Test] public void should_return_null_result () =>
            result.Should().BeNull();


        private static ImportDataParsingService importDataParsingService;
        private static QuestionnaireDocument questionnaireDocument;
        private static int[] result;
    }
}
