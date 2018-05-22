using System;
using System.Threading;
using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.StataEnvironmentContentGeneratorTests
{
    internal class when_HeaderStructureForLevel_has_no_questions : StataEnvironmentContentGeneratorTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            emptyHeaderStructureForLevel = CreateHeaderStructureForLevel(dataFileName);
            questionnaireExportStructure = Create.Entity.QuestionnaireExportStructure();
            questionnaireExportStructure.HeaderToLevelMap.Add(new ValueVector<Guid>(),
                emptyHeaderStructureForLevel);

            stataEnvironmentContentService = CreateStataEnvironmentContentGenerator(CreateFileSystemAccessor((c) => stataGeneratedContent = c));
            BecauseOf();
        }

        public void BecauseOf() => stataEnvironmentContentService.CreateEnvironmentFiles(questionnaireExportStructure, "",
                    default(CancellationToken)); //stataEnvironmentContentService.CreateContentOfAdditionalFile(emptyHeaderStructureForLevel,dataFileName, contentFilePath);

        [NUnit.Framework.Test] public void should_contain_stata_script_for_insheet_file () =>
            stataGeneratedContent.Should().Contain($"insheet using \"{dataFileName}.tab\", tab case names\r\n");

        private static StataEnvironmentContentService stataEnvironmentContentService;
        private static HeaderStructureForLevel emptyHeaderStructureForLevel;
        private static string dataFileName="data file name";
        private static string stataGeneratedContent;
      //  private static string contentFilePath = "content file path";
        private static QuestionnaireExportStructure questionnaireExportStructure;
    }
}
