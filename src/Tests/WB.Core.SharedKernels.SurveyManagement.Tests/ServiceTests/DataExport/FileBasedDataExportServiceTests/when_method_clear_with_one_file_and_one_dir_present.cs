using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.ServiceTests.DataExport.FileBasedDataExportServiceTests
{
    class when_method_clear_with_one_file_and_one_dir_present : FileBasedDataExportServiceTestContext
    {
        Establish context = () =>
        {
            fileSystemAccessorMock=new Mock<IFileSystemAccessor>();
            
            fileSystemAccessorMock.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);
            fileSystemAccessorMock.Setup(x => x.GetFilesInDirectory(Moq.It.Is<string>(name => name.Contains("ExportedData")))).Returns(new[] { "f1" });
            fileSystemAccessorMock.Setup(x => x.GetDirectoriesInDirectory(Moq.It.Is<string>(name=> name.Contains("ExportedData")))).Returns(new[] { "d1"});

            fileSystemAccessorMock.Setup(x => x.GetFilesInDirectory(Moq.It.Is<string>(name => name.Contains("ExportedFiles")))).Returns(new[] { "f2" });
            fileSystemAccessorMock.Setup(x => x.GetDirectoriesInDirectory(Moq.It.Is<string>(name => name.Contains("ExportedFiles")))).Returns(new[] { "d2" });

            fileBasedDataExportService = CreateFileBasedDataExportService(fileSystemAccessorMock.Object);
        };
        
        Because of = () => fileBasedDataExportService.Clear();

        It should_call_delete_method_for_present_file_in_exported_data = () =>
            fileSystemAccessorMock.Verify(x=>x.DeleteFile("f1"), Times.Once());

        It should_call_delete_method_for_present_directory_in_exported_data = () =>
            fileSystemAccessorMock.Verify(x => x.DeleteDirectory("d1"), Times.Once());

        It should_call_delete_method_for_present_file_in_exported_files = () =>
            fileSystemAccessorMock.Verify(x => x.DeleteFile("f2"), Times.Once());

        It should_call_delete_method_for_present_directory_in_exported_files = () =>
            fileSystemAccessorMock.Verify(x => x.DeleteDirectory("d2"), Times.Once());

        private static FileBasedDataExportService fileBasedDataExportService;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
    }
}
