using System;
using System.Threading;
using NUnit.Framework;
using WB.Services.Export.CsvExport.Implementation.DoFiles;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.CsvExport.Implementation.DoFiles
{
    
    internal class StataEnvironmentContentGeneratorTests : StataEnvironmentContentGeneratorTestContext
    {
        [Test]
        public void when_HeaderStructureForLevel_has_illegal_level_labels()
        {
            string dataFileName = "data file name";

            string stataGeneratedContent = "";

            var oneQuestionHeaderStructureForLevel = CreateHeaderStructureForLevel(dataFileName);
            oneQuestionHeaderStructureForLevel.LevelLabels =
                new[] {CreateLabelItem("c1", "t1"), CreateLabelItem("c2", "t2")};


            var questionnaireExportStructure = Create.QuestionnaireExportStructure("questionnaire");
            questionnaireExportStructure.HeaderToLevelMap.Add(new ValueVector<Guid>(),
                oneQuestionHeaderStructureForLevel);

            var stataEnvironmentContentService = CreateStataEnvironmentContentGenerator(CreateFileSystemAccessor((c) => stataGeneratedContent = c));
            stataEnvironmentContentService.CreateEnvironmentFiles(questionnaireExportStructure, "",
                default(CancellationToken));

            Assert.That(stataGeneratedContent.Contains(string.Format("insheet using \"{0}.tab\", tab case names\r\n", dataFileName)));
            Assert.That(stataGeneratedContent.Contains("label values Id Id"));
            Assert.That(stataGeneratedContent.Contains("/*label define Id*/ /*c1 `\"t1\"'*/ /*c2 `\"t2\"'*/"));
        }
        [Test]
        public void when_HeaderStructureForLevel_has_level_labels ()
        {
            StataEnvironmentContentService stataEnvironmentContentService;
            HeaderStructureForLevel oneQuestionHeaderStructureForLevel;
            string dataFileName = "data file name";
            string stataGeneratedContent = "";
            QuestionnaireExportStructure questionnaireExportStructure;

            oneQuestionHeaderStructureForLevel =
                    CreateHeaderStructureForLevel(dataFileName);
                oneQuestionHeaderStructureForLevel.LevelLabels = new[] { CreateLabelItem("1", "t1"), CreateLabelItem("2", "t2") };

                questionnaireExportStructure = Create.QuestionnaireExportStructure("questionnaire");
                questionnaireExportStructure.HeaderToLevelMap.Add(new ValueVector<Guid>(),
                    oneQuestionHeaderStructureForLevel);

                stataEnvironmentContentService =
                    CreateStataEnvironmentContentGenerator(CreateFileSystemAccessor((c) => stataGeneratedContent = c));
            

             stataEnvironmentContentService.CreateEnvironmentFiles(questionnaireExportStructure, "",
                        default(CancellationToken));

            Assert.That(stataGeneratedContent.Contains(string.Format("insheet using \"{0}.tab\", tab case names\r\n", dataFileName)));
            Assert.That(stataGeneratedContent.Contains(string.Format("label values Id Id")));
            Assert.That(stataGeneratedContent.Contains(string.Format("label define Id 1 `\"t1\"' 2 `\"t2\"'")));
        }

        [Test]
        public void when_HeaderStructureForLevel_has_one_question_with_labels()
        {
            string dataFileName = "data file name";

            string questionsVariableName = "var1";
            string questionsTitle = "title1";
            string stataGeneratedContent = "";

            var oneQuestionHeaderStructureForLevel = CreateHeaderStructureForLevel(dataFileName, exportedQuestionHeaderItems: new[] { CreateExportedHeaderItem(questionsVariableName, questionsTitle, CreateLabelItem("1", "t1`\r"), CreateLabelItem("2", "t2\r'")) });

                var questionnaireExportStructure = Create.QuestionnaireExportStructure("questionnaire");
                questionnaireExportStructure.HeaderToLevelMap.Add(new ValueVector<Guid>(),
                    oneQuestionHeaderStructureForLevel);

                var stataEnvironmentContentService = CreateStataEnvironmentContentGenerator(CreateFileSystemAccessor((c) => stataGeneratedContent = c));
            
            stataEnvironmentContentService.CreateEnvironmentFiles(questionnaireExportStructure, "", default(CancellationToken))/*.CreateContentOfAdditionalFile(oneQuestionHeaderStructureForLevel,dataFileName, contentFilePath)*/;

            Assert.That(stataGeneratedContent.Contains(string.Format("insheet using \"{0}.tab\", tab case names\r\n", dataFileName)));
            Assert.That(stataGeneratedContent.Contains(string.Format("label variable {0} `\"{1}\"'", questionsVariableName, questionsTitle)));
            Assert.That(stataGeneratedContent.Contains(string.Format("label values {0} {0}", questionsVariableName)));
            Assert.That(stataGeneratedContent.Contains($"label define {questionsVariableName} 1 `\"t1\"' 2 `\"t2\"'"));
        }
    }
}
