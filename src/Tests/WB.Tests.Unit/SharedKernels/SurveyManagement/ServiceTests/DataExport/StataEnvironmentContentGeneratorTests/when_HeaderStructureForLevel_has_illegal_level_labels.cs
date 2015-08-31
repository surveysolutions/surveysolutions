﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.StataEnvironmentContentGeneratorTests
{
    internal class when_HeaderStructureForLevel_has_illegal_level_labels : StataEnvironmentContentGeneratorTestContext
    {
        Establish context = () =>
        {
            oneQuestionHeaderStructureForLevel =
                CreateHeaderStructureForLevel();
            oneQuestionHeaderStructureForLevel.LevelLabels = new[] { CreateLabelItem("c1", "t1"), CreateLabelItem("c2", "t2") };

            stataEnvironmentContentService =
                CreateStataEnvironmentContentGenerator(CreateFileSystemAccessor((c) => stataGeneratedContent = c));
        };

        Because of = () => stataEnvironmentContentService.CreateContentOfAdditionalFile(oneQuestionHeaderStructureForLevel,dataFileName, contentFilePath);

        It should_contain_stata_script_for_insheet_file = () =>
            stataGeneratedContent.ShouldContain(string.Format("insheet using \"{0}\", tab\r\n", dataFileName));

        It should_contain_stata_id_variable_on_ids_label_mapping = () =>
           stataGeneratedContent.ShouldContain(string.Format("label values {0} l{0}", oneQuestionHeaderStructureForLevel.LevelIdColumnName));

        It should_contain_label_definition_for_id = () =>
            stataGeneratedContent.ShouldContain(string.Format("/*label define l{0}*/ /*c1 `\"t1\"'*/ /*c2 `\"t2\"'*/", oneQuestionHeaderStructureForLevel.LevelIdColumnName));

        private static StataEnvironmentContentService stataEnvironmentContentService;
        private static HeaderStructureForLevel oneQuestionHeaderStructureForLevel;
        private static string dataFileName = "data file name";
        private static string contentFilePath = "content file path";
        private static string stataGeneratedContent;
    }
}
