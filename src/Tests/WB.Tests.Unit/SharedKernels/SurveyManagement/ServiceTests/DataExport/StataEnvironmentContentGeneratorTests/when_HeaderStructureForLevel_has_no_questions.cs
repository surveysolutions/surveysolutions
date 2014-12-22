using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.StataEnvironmentContentGeneratorTests
{
    internal class when_HeaderStructureForLevel_has_no_questions : StataEnvironmentContentGeneratorTestContext
    {
        Establish context = () =>
        {
            emptyHeaderStructureForLevel = CreateHeaderStructureForLevel();
            stataEnvironmentContentService = CreateStataEnvironmentContentGenerator(CreateFileSystemAccessor((c) => stataGeneratedContent = c));
        };

        Because of = () => stataEnvironmentContentService.CreateContentOfAdditionalFile(emptyHeaderStructureForLevel,dataFileName, contentFilePath);

        It should_contain_stata_script_for_insheet_file = () =>
            stataGeneratedContent.ShouldEqual(string.Format("insheet using \"{0}\", tab\r\nlist\r\n", dataFileName));

        private static StataEnvironmentContentService stataEnvironmentContentService;
        private static HeaderStructureForLevel emptyHeaderStructureForLevel;
        private static string dataFileName="data file name";
        private static string stataGeneratedContent;
        private static string contentFilePath = "content file path";
    }
}
