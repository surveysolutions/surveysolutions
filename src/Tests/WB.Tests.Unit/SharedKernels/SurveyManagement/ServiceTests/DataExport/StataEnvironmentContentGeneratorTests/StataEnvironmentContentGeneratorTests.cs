using System;
using System.Threading;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.StataEnvironmentContentGeneratorTests
{
    
    internal class StataEnvironmentContentGeneratorTests : StataEnvironmentContentGeneratorTestContext
    {
        [Test]
        public void when_HeaderStructureForLevel_has_illegal_level_labels()
        {
            StataEnvironmentContentService stataEnvironmentContentService;
            HeaderStructureForLevel oneQuestionHeaderStructureForLevel;

            string dataFileName = "data file name";

            string stataGeneratedContent = "";
            QuestionnaireExportStructure questionnaireExportStructure;

            oneQuestionHeaderStructureForLevel = CreateHeaderStructureForLevel(dataFileName);
            oneQuestionHeaderStructureForLevel.LevelLabels =
                new[] {CreateLabelItem("c1", "t1"), CreateLabelItem("c2", "t2")};


            questionnaireExportStructure = Create.Entity.QuestionnaireExportStructure();
            questionnaireExportStructure.HeaderToLevelMap.Add(new ValueVector<Guid>(),
                oneQuestionHeaderStructureForLevel);

            stataEnvironmentContentService =
                CreateStataEnvironmentContentGenerator(CreateFileSystemAccessor((c) => stataGeneratedContent = c));
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

                questionnaireExportStructure = Create.Entity.QuestionnaireExportStructure();
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
            StataEnvironmentContentService stataEnvironmentContentService;
            HeaderStructureForLevel oneQuestionHeaderStructureForLevel;
            QuestionnaireExportStructure questionnaireExportStructure;
            string dataFileName = "data file name";

            string questionsVariableName = "var1";
            string questionsTitle = "title1";
            string stataGeneratedContent = "";

            oneQuestionHeaderStructureForLevel =
                    CreateHeaderStructureForLevel(dataFileName, exportedQuestionHeaderItems: new[] { CreateExportedHeaderItem(questionsVariableName, questionsTitle, CreateLabelItem("1", "t1`\r"), CreateLabelItem("2", "t2\r'")) });

                questionnaireExportStructure = Create.Entity.QuestionnaireExportStructure();
                questionnaireExportStructure.HeaderToLevelMap.Add(new ValueVector<Guid>(),
                    oneQuestionHeaderStructureForLevel);

                stataEnvironmentContentService = CreateStataEnvironmentContentGenerator(CreateFileSystemAccessor((c) => stataGeneratedContent = c));
            
            stataEnvironmentContentService.CreateEnvironmentFiles(questionnaireExportStructure, "", default(CancellationToken))/*.CreateContentOfAdditionalFile(oneQuestionHeaderStructureForLevel,dataFileName, contentFilePath)*/;

            Assert.That(stataGeneratedContent.Contains(string.Format("insheet using \"{0}.tab\", tab case names\r\n", dataFileName)));
            Assert.That(stataGeneratedContent.Contains(string.Format("label variable {0} `\"{1}\"'", questionsVariableName, questionsTitle)));
            Assert.That(stataGeneratedContent.Contains(string.Format("label values {0} {0}", questionsVariableName)));
            Assert.That(stataGeneratedContent.Contains($"label define {questionsVariableName} 1 `\"t1\"' 2 `\"t2\"'"));
        }
    }
}
