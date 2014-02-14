using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.Infrastructure.FileSystem;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.ServiceTests.DataExport.FileBasedDataExportServiceTests
{
    internal class when_AddExportedDataByInterview_is_called_and_directory_is_present : FileBasedDataExportServiceTestContext
    {
        Establish context = () =>
        {
            interviewExportServiceMock = new Mock<IInterviewExportService>();
            var fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(true);
            fileSystemAccessorMock.Setup(x => x.MakeValidFileName(Moq.It.IsAny<string>())).Returns("1st");

            interviewLevelToExport = new InterviewDataExportLevelView(Guid.NewGuid(), "1st", null);
            interviewToExport = new InterviewDataExportView(Guid.NewGuid(), 1,
                new[] { interviewLevelToExport });
            fileBasedDataExportService = CreateFileBasedDataExportService(fileSystemAccessorMock.Object, interviewExportServiceMock.Object);
        };

        Because of = () =>
            fileBasedDataExportService.AddExportedDataByInterview(interviewToExport);

        It should_data_file_name_be_requested_once = () =>
            interviewExportServiceMock.Verify(x => x.GetInterviewExportedDataFileName("1st"), Times.Once());

        It should_data_by_level_be_stored_once = () =>
           interviewExportServiceMock.Verify(x => x.AddRecord(interviewLevelToExport,Moq.It.IsAny<string>()), Times.Once());

        private static FileBasedDataExportService fileBasedDataExportService;
        
        private static Mock<IInterviewExportService> interviewExportServiceMock;
        private static InterviewDataExportView interviewToExport;
        private static InterviewDataExportLevelView interviewLevelToExport;
    }
}
