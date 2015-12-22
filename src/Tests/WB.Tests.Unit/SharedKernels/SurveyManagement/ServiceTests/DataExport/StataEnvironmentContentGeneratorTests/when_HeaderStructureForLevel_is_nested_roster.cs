using System;
using System.Threading;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.StataEnvironmentContentGeneratorTests
{
    internal class when_HeaderStructureForLevel_is_nested_roster : StataEnvironmentContentGeneratorTestContext
    {
        Establish context = () =>
        {
            questionnaireExportStructure = Create.QuestionnaireExportStructure();

            var topHeaderStructureForLevel =
                CreateHeaderStructureForLevel("top");
            topHeaderStructureForLevel.LevelScopeVector = new ValueVector<Guid>();

            var parentRosterId = Guid.NewGuid();
            var parentRosterHeaderStructureForLevel =
                CreateHeaderStructureForLevel("parent");
            parentRosterHeaderStructureForLevel.LevelScopeVector = new ValueVector<Guid>(new[] { questionnaireExportStructure.QuestionnaireId });

            nestedRosterHeaderStructureForLevel =
                CreateHeaderStructureForLevel(dataFileName);
            nestedRosterHeaderStructureForLevel.LevelScopeVector =
                new ValueVector<Guid>(new[] {questionnaireExportStructure.QuestionnaireId, parentRosterId});

            questionnaireExportStructure.HeaderToLevelMap.Add(topHeaderStructureForLevel.LevelScopeVector,
                topHeaderStructureForLevel);
            questionnaireExportStructure.HeaderToLevelMap.Add(parentRosterHeaderStructureForLevel.LevelScopeVector,
                parentRosterHeaderStructureForLevel);
            questionnaireExportStructure.HeaderToLevelMap.Add(nestedRosterHeaderStructureForLevel.LevelScopeVector,
                nestedRosterHeaderStructureForLevel);

            stataEnvironmentContentService =
                CreateStataEnvironmentContentGenerator(CreateFileSystemAccessor((c) => stataGeneratedContent = c));
        };

        Because of =
            () =>
                stataEnvironmentContentService.CreateEnvironmentFiles(questionnaireExportStructure, "",
                    default(CancellationToken));

        It should_contain_stata_script_for_insheet_file = () =>
            stataGeneratedContent.ShouldContain(string.Format("insheet using \"{0}.tab\", tab\r\n", dataFileName));

        It should_contain_stata_variable_parent2_on_InterviewId_mapping = () =>
            stataGeneratedContent.ShouldContain("label variable parentid2 `\"interviewid\"'");

        It should_contain_stata_variable_parent1_on_parent_roster_mapping = () =>
            stataGeneratedContent.ShouldContain("label variable parentid1 `\"id in \"parent\"\"'");

        private static StataEnvironmentContentService stataEnvironmentContentService;
        private static HeaderStructureForLevel nestedRosterHeaderStructureForLevel;
        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static string dataFileName = "data file name";
        
        private static string stataGeneratedContent;
    }
}