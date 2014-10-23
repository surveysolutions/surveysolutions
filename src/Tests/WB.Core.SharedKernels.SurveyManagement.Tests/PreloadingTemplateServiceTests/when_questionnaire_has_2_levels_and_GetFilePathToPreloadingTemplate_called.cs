using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.PreloadingTemplateServiceTests
{
    internal class when_questionnaire_has_2_levels_and_GetFilePathToPreloadingTemplate_called : PreloadingTemplateServiceTestContext
    {
        Establish context = () =>
        {
            exportedDataFormatter=new Mock<IExportedDataFormatter>();
            fileSystemAccessor = CreateIFileSystemAccessorMock();
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(Moq.It.IsAny<string>())).Returns(new[] { "1.tab" });
            preloadingTemplateService = CreatePreloadingTemplateService(fileSystemAccessor.Object,
                exportedDataFormatter: exportedDataFormatter.Object);
        };

        Because of = () => result = preloadingTemplateService.GetFilePathToPreloadingTemplate(questionnaireId, 1);

        It should_return_not_null_result = () =>
           result.ShouldNotBeNull();

        It should_return_valid_archive_name = () =>
            result.ShouldEndWith(string.Format("template_{0}_v{1}.zip", questionnaireId.FormatGuid(), 1));

        It should_template_for_preloade_be_created_once = () =>
            exportedDataFormatter.Verify(x => x.CreateHeaderStructureForPreloadingForQuestionnaire(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>(), Moq.It.IsAny<string>()), Times.Once);

        private static PreloadingTemplateService preloadingTemplateService;
        private static string result;
        private static Mock<IFileSystemAccessor> fileSystemAccessor;
        private static Mock<IExportedDataFormatter> exportedDataFormatter;
        private static Guid questionnaireId = Guid.NewGuid();
    }
}
