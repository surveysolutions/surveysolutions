using System;
using System.Threading;
using NUnit.Framework;
using WB.Services.Export.CsvExport.Implementation.DoFiles;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.CsvExport.Implementation.DoFiles
{
    internal class when_HeaderStructureForLevel_has_one_multi_option_question : StataEnvironmentContentGeneratorTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var multiOptionQuestion =
                    Create.MultyOptionsQuestion(id: Guid.NewGuid(), variable: questionsVariableName,
                        options: new[] { Create.Option("one", "1"), Create.Option("three", "3") });
            multiOptionQuestion.QuestionText = questionsTitle;
            var questionnaireDocument = Create.QuestionnaireDocument(children: new[]
            {
                multiOptionQuestion
            });
            questionnaireDocument.Title = dataFileName;

            var fileSystemAccessor = CreateFileSystemAccessor((c) => stataGeneratedContent = c);

            questionnaireExportStructure =
                Create.QuestionnaireExportStructure(questionnaireDocument);

            stataEnvironmentContentService = CreateStataEnvironmentContentGenerator(fileSystemAccessor);
            BecauseOf();
        }

        public void BecauseOf() => stataEnvironmentContentService.CreateEnvironmentFiles(questionnaireExportStructure, "", default(CancellationToken));

        [NUnit.Framework.Test] public void should_contain_stata_script_for_insheet_file () =>
            Assert.That(stataGeneratedContent, Does.Contain(string.Format("insheet using \"{0}.tab\", tab case names\r\n", dataFileName)));

        [NUnit.Framework.Test] public void should_contain_stata_variable_on_title_mapping_for_first_option () =>
            Assert.That(stataGeneratedContent, Does.Contain("label variable var1__1 `\"title1:one\"'"));

        [NUnit.Framework.Test] public void should_contain_stata_variable_on_title_mapping_for_second_option () =>
            Assert.That(stataGeneratedContent, Does.Contain("label variable var1__3 `\"title1:three\"'"));

        [NUnit.Framework.Test] public void should_not_contain_stata_variable_on_label_mapping_for_first_option () =>
            Assert.That(stataGeneratedContent, Does.Not.Contain("label values var1__1 lvar1__1"));

        [NUnit.Framework.Test] public void should_not_contain_stata_variable_on_label_mapping_for_second_option () =>
            Assert.That(stataGeneratedContent, Does.Not.Contain("label values var1__3 lvar1__3"));

        [NUnit.Framework.Test] public void should_not_contain_label_definition () =>
            Assert.That(stataGeneratedContent, Does.Not.Contain("label define lvar1"));

        private static StataEnvironmentContentService stataEnvironmentContentService;
        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static string dataFileName = "data file name";

        private static string questionsVariableName = "var1";
        private static string questionsTitle = "title1";
        private static string stataGeneratedContent;
    }
}
