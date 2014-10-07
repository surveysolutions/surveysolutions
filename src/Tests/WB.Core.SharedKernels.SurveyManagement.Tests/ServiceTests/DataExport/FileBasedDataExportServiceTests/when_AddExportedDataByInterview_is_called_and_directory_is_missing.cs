using System;
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

namespace WB.Core.SharedKernels.SurveyManagement.Tests.ServiceTests.DataExport.FileBasedDataExportServiceTests
{
    internal class when_AddExportedDataByInterview_is_called_and_directory_is_missing : FileBasedDataExportServiceTestContext
    {
        Establish context = () =>
        {
            var fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(false);
            fileBasedDataExportService = CreateFileBasedDataExportService(fileSystemAccessorMock.Object);
        };

        Because of =()=>
            raisedException = Catch.Exception(() => fileBasedDataExportService.AddExportedDataByInterview(new InterviewDataExportView(Guid.NewGuid(), Guid.NewGuid(), 1, null))) as InterviewDataExportException;

        It should_InterviewDataExportException_be_rised = () =>
            raisedException.ShouldNotBeNull();

        It should_rised_exception_has_message_about_missing_folder = () =>
            raisedException.Message.ShouldContain("data files are absent for questionnaire");

        private static FileBasedDataExportService fileBasedDataExportService;
        private static InterviewDataExportException raisedException;
    }
}
