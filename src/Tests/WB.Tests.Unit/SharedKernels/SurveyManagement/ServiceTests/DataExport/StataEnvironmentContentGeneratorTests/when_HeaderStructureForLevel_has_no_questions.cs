using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.StataEnvironmentContentGeneratorTests
{
    internal class when_HeaderStructureForLevel_has_no_questions : StataEnvironmentContentGeneratorTestContext
    {
        Establish context = () =>
        {
            emptyHeaderStructureForLevel = CreateHeaderStructureForLevel(dataFileName);
            questionnaireExportStructure = Create.QuestionnaireExportStructure();
            questionnaireExportStructure.HeaderToLevelMap.Add(new ValueVector<Guid>(),
                emptyHeaderStructureForLevel);

            stataEnvironmentContentService = CreateStataEnvironmentContentGenerator(CreateFileSystemAccessor((c) => stataGeneratedContent = c));
        };

        Because of = () => stataEnvironmentContentService.CreateEnvironmentFiles(questionnaireExportStructure, "",
                    default(CancellationToken)); //stataEnvironmentContentService.CreateContentOfAdditionalFile(emptyHeaderStructureForLevel,dataFileName, contentFilePath);

        It should_contain_stata_script_for_insheet_file = () =>
            stataGeneratedContent.ShouldContain($"insheet using \"{dataFileName}.tab\", tab\r\n");

        private static StataEnvironmentContentService stataEnvironmentContentService;
        private static HeaderStructureForLevel emptyHeaderStructureForLevel;
        private static string dataFileName="data file name";
        private static string stataGeneratedContent;
      //  private static string contentFilePath = "content file path";
        private static QuestionnaireExportStructure questionnaireExportStructure;
    }
}
