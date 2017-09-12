using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.StataEnvironmentContentGeneratorTests
{
    internal class when_HeaderStructureForLevel_has_one_question_with_no_labels : StataEnvironmentContentGeneratorTestContext
    {
        Establish context = () =>
        {
            oneQuestionHeaderStructureForLevel =
                CreateHeaderStructureForLevel(dataFileName, exportedQuestionHeaderItems: new [] { CreateExportedHeaderItem(questionsVariableName, questionsTitle)});
            questionnaireExportStructure = Create.Entity.QuestionnaireExportStructure();
            questionnaireExportStructure.HeaderToLevelMap.Add(new ValueVector<Guid>(),
                oneQuestionHeaderStructureForLevel);
            stataEnvironmentContentService =
                CreateStataEnvironmentContentGenerator(CreateFileSystemAccessor((c) => stataGeneratedContent = c));
        };

        Because of =
            () =>
                stataEnvironmentContentService.CreateEnvironmentFiles(questionnaireExportStructure, "",
                    default(CancellationToken));//stataEnvironmentContentService.CreateContentOfAdditionalFile(oneQuestionHeaderStructureForLevel,dataFileName, contentFilePath);

        It should_contain_stata_script_for_insheet_file = () =>
            stataGeneratedContent.ShouldContain(string.Format("insheet using \"{0}.tab\", tab\r\n", dataFileName));

        It should_contain_stata_variable_on_title_mapping = () =>
           stataGeneratedContent.ShouldContain(string.Format("label variable {0} `\"{1}\"'",questionsVariableName,questionsTitle));

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
