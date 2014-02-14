using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport;
using WB.Core.Infrastructure.FileSystem;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.ServiceTests.DataExport.FileBasedDataExportServiceTests
{
    class when_method_clear_with_one_file_and_one_dir_present : FileBasedDataExportServiceTestContext
    {
        Establish context = () =>
        {
            fileSystemAccessorMock=new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.GetFilesInDirectory(Moq.It.IsAny<string>())).Returns(new[] { "f1" });
            fileSystemAccessorMock.Setup(x => x.GetDirectoriesInDirectory(Moq.It.IsAny<string>())).Returns(new[] { "d1"});
            fileBasedDataExportService = CreateFileBasedDataExportService(fileSystemAccessorMock.Object);
        };
        
        Because of = () => fileBasedDataExportService.Clear();

        It should_call_delete_method_for_present_file = () =>
            fileSystemAccessorMock.Verify(x=>x.DeleteFile("f1"), Times.Once());

        It should_call_delete_method_for_present_directory = () =>
            fileSystemAccessorMock.Verify(x => x.DeleteDirectory("d1"), Times.Once());

        private static FileBasedDataExportService fileBasedDataExportService;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
    }
}
