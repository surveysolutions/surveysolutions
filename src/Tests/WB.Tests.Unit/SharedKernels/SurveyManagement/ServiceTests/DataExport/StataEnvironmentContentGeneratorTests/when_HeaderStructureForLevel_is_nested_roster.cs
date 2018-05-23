using System;
using System.Threading;
using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.StataEnvironmentContentGeneratorTests
{
    internal class when_HeaderStructureForLevel_is_nested_roster : StataEnvironmentContentGeneratorTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireExportStructure = Create.Entity.QuestionnaireExportStructure();

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
            BecauseOf();
        }

        private void BecauseOf() =>
                stataEnvironmentContentService.CreateEnvironmentFiles(questionnaireExportStructure, "",
                    default(CancellationToken));

        [NUnit.Framework.Test] public void should_contain_stata_script_for_insheet_file () =>
            stataGeneratedContent.Should().Contain(string.Format("insheet using \"{0}.tab\", tab case names\r\n", dataFileName));

        [NUnit.Framework.Test] public void should_contain_stata_variable_parent2_on_InterviewId_mapping () =>
            stataGeneratedContent.Should().Contain("label variable interview__id `\"InterviewId\"'");

        [NUnit.Framework.Test] public void should_contain_stata_variable_parent1_on_parent_roster_mapping () =>
            stataGeneratedContent.Should().Contain("label variable parent__id `\"Id in \"parent\"\"'");

        private static StataEnvironmentContentService stataEnvironmentContentService;
        private static HeaderStructureForLevel nestedRosterHeaderStructureForLevel;
        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static string dataFileName = "data file name";
        
        private static string stataGeneratedContent;
    }
}
