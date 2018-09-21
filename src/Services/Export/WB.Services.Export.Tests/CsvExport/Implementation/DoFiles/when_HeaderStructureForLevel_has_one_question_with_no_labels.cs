using System;
using System.Threading;
using NUnit.Framework;
using WB.Services.Export.CsvExport.Implementation.DoFiles;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.CsvExport.Implementation.DoFiles
{
    internal class when_HeaderStructureForLevel_has_one_question_with_no_labels : StataEnvironmentContentGeneratorTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            oneQuestionHeaderStructureForLevel =
                CreateHeaderStructureForLevel(dataFileName, exportedQuestionHeaderItems: new [] { CreateExportedHeaderItem(questionsVariableName, questionsTitle)});
            questionnaireExportStructure = Create.QuestionnaireExportStructure();
            questionnaireExportStructure.HeaderToLevelMap.Add(new ValueVector<Guid>(),
                oneQuestionHeaderStructureForLevel);
            stataEnvironmentContentService =
                CreateStataEnvironmentContentGenerator(CreateFileSystemAccessor((c) => stataGeneratedContent = c));
            BecauseOf();
        }

        private void BecauseOf() =>
                stataEnvironmentContentService.CreateEnvironmentFiles(questionnaireExportStructure, "",
                    default(CancellationToken));//stataEnvironmentContentService.CreateContentOfAdditionalFile(oneQuestionHeaderStructureForLevel,dataFileName, contentFilePath);

        [NUnit.Framework.Test] public void should_contain_stata_script_for_insheet_file () =>
            Assert.That(stataGeneratedContent, Does.Contain(string.Format("insheet using \"{0}.tab\", tab case names\r\n", dataFileName)));

        [NUnit.Framework.Test] public void should_contain_stata_variable_on_title_mapping () =>
          Assert.That(stataGeneratedContent, Does.Contain(string.Format("label variable {0} `\"{1}\"'",questionsVariableName,questionsTitle)));

        private static StataEnvironmentContentService stataEnvironmentContentService;
        private static HeaderStructureForLevel oneQuestionHeaderStructureForLevel;
        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static string dataFileName = "data file name";

        private static string questionsVariableName = "var1";
        private static string questionsTitle = "title1";
        private static string stataGeneratedContent;
      //  private static string contentFilePath = "content file path";
    }
}
