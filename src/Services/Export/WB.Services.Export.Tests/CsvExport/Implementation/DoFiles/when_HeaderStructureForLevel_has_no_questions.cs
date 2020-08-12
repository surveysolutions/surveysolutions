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
        [Test]
        public void should_contain_stata_script_for_insheet_file()
        {
            emptyHeaderStructureForLevel = CreateHeaderStructureForLevel(dataFileName);
            questionnaireExportStructure = Create.QuestionnaireExportStructure();
            questionnaireExportStructure.HeaderToLevelMap.Add(new ValueVector<Guid>(),
                emptyHeaderStructureForLevel);

            stataEnvironmentContentService =
                CreateStataEnvironmentContentGenerator(CreateFileSystemAccessor((c) => stataGeneratedContent = c));
            
            // Act
            stataEnvironmentContentService.CreateEnvironmentFiles(questionnaireExportStructure,
                "",
                default(CancellationToken));

            Assert.That(stataGeneratedContent,
                Does.Contain($"insheet using \"{dataFileName}.tab\", tab case names{Environment.NewLine}"));
        }

        private StataEnvironmentContentService stataEnvironmentContentService;
        private HeaderStructureForLevel emptyHeaderStructureForLevel;
        private string dataFileName = "data file name";
        private string stataGeneratedContent;

        private QuestionnaireExportStructure questionnaireExportStructure;
    }
}
