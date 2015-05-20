using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadingTemplateServiceTests
{
    internal class when_archive_were_build_before_GetFilePathToPreloadingTemplate_called : PreloadingTemplateServiceTestContext
    {
        Establish context = () =>
        {
            fileSystemAccessor = CreateIFileSystemAccessorMock();
            fileSystemAccessor.Setup(x => x.IsFileExists(Moq.It.IsAny<string>())).Returns(true);
            dataFileExportService = CreateIDataFileExportServiceMock();
            preloadingTemplateService = CreatePreloadingTemplateService(fileSystemAccessor.Object);
        };

        Because of = () => result = preloadingTemplateService.GetFilePathToPreloadingTemplate(questionnaireId, 1);

        It should_return_not_null_result = () =>
           result.ShouldNotBeNull();

        It should_return_valid_archive_name = () =>
            result.ShouldEndWith(string.Format("template_{0}_v{1}.zip", questionnaireId.FormatGuid(), 1));

        It should_header_be_created_zero_times = () =>
           dataFileExportService.Verify(x => x.CreateStructure(Moq.It.IsAny<QuestionnaireExportStructure>(), Moq.It.IsAny<string>()), Times.Never);

        private static PreloadingTemplateService preloadingTemplateService;
        private static string result;
        private static Mock<IFileSystemAccessor> fileSystemAccessor;
        private static Mock<IDataExportWriter> dataFileExportService;
        private static Guid questionnaireId = Guid.NewGuid();
    }
}
