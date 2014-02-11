using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;

namespace WB.Core.BoundedContexts.Supervisor.Tests.ServiceTests.DataExport.StataEnvironmentContentGeneratorTests
{
    internal class when_HeaderStructureForLevel_has_no_questions : StataEnvironmentContentGeneratorTestContext
    {
        Establish context = () =>
        {
            emptyHeaderStructureForLevel = CreateHeaderStructureForLevel();
            stataEnvironmentContentGenerator = CreateStataEnvironmentContentGenerator(emptyHeaderStructureForLevel, dataFileName);
        };

        Because of = () => stataGeneratedContent = stataEnvironmentContentGenerator.ContentOfAdditionalFile;

        It should_contain_stata_script_for_insheet_file = () =>
            stataGeneratedContent.ShouldEqual(string.Format("insheet using \"{0}\", comma\r\nlist\r\n", dataFileName));

        private static StataEnvironmentContentGenerator stataEnvironmentContentGenerator;
        private static HeaderStructureForLevel emptyHeaderStructureForLevel;
        private static string dataFileName="data file name";
        private static string stataGeneratedContent;
    }
}
