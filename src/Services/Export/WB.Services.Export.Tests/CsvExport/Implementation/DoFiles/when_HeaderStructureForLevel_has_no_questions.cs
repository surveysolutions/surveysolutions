using System;
using System.Threading;
using NUnit.Framework;
using WB.Services.Export.CsvExport.Implementation.DoFiles;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.CsvExport.Implementation.DoFiles
{
    internal class when_HeaderStructureForLevel_has_no_questions : StataEnvironmentContentGeneratorTestContext
    {
        [OneTimeSetUp] public void context () {
            emptyHeaderStructureForLevel = CreateHeaderStructureForLevel(dataFileName);
            questionnaireExportStructure = Create.QuestionnaireExportStructure();
            questionnaireExportStructure.HeaderToLevelMap.Add(new ValueVector<Guid>(),
                emptyHeaderStructureForLevel);

            stataEnvironmentContentService = CreateStataEnvironmentContentGenerator(CreateFileSystemAccessor((c) => stataGeneratedContent = c));
            BecauseOf();
        }

        public void BecauseOf() => stataEnvironmentContentService.CreateEnvironmentFiles(questionnaireExportStructure, "",
                    default(CancellationToken)); //stataEnvironmentContentService.CreateContentOfAdditionalFile(emptyHeaderStructureForLevel,dataFileName, contentFilePath);

        [NUnit.Framework.Test] public void should_contain_stata_script_for_insheet_file () =>
            Assert.That(stataGeneratedContent, Does.Contain($"insheet using \"{dataFileName}.tab\", tab case names\r\n"));

        private static StataEnvironmentContentService stataEnvironmentContentService;
        private static HeaderStructureForLevel emptyHeaderStructureForLevel;
        private static string dataFileName="data file name";
        private static string stataGeneratedContent;
      //  private static string contentFilePath = "content file path";
        private static QuestionnaireExportStructure questionnaireExportStructure;
    }
}
