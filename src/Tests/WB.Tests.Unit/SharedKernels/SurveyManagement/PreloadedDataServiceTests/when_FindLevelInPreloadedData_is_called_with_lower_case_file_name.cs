using System;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    internal class when_FindLevelInPreloadedData_is_called_with_lower_case_file_name : PreloadedDataServiceTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(
                    Create.Entity.FixedRoster(rosterId: rosterGroupId,
                        obsoleteFixedTitles: new[] {"1"}, title: "Roster Group", variable: "roster"));

            importDataParsingService = CreatePreloadedDataService(questionnaireDocument);
            BecauseOf();
        }

        private void BecauseOf() => result = importDataParsingService.FindLevelInPreloadedData("roster");

        [NUnit.Framework.Test]
        public void should_return_not_null_result() =>
            result.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_result_levelId_be_equal_to_rosterGroupId () =>
          result.LevelScopeVector.Should().BeEquivalentTo(new[] { rosterGroupId });

        private static ImportDataParsingService importDataParsingService;
        private static QuestionnaireDocument questionnaireDocument;
        private static HeaderStructureForLevel result;
        private static Guid rosterGroupId = Guid.NewGuid();
    }
}
