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
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.ServiceTests.DataExport.FileBasedDataExportServiceTests
{
    internal class when_CreateExportedDataStructureByTemplate_is_called_and_directory_is_present : FileBasedDataExportServiceTestContext
    {
        Establish context = () =>
        {
            dataFileExportServiceMock=new Mock<IDataFileExportService>();
            dataFileExportServiceMock.Setup(x => x.GetInterviewActionFileName()).Returns("file.csv");

            fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.IsDirectoryExists(it.IsAny<string>())).Returns(true);

            fileSystemAccessorMock.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);

            fileBasedDataExportService = CreateFileBasedDataExportService(fileSystemAccessorMock.Object, dataFileExportServiceMock.Object);
        };

        Because of = () =>
            raisedException =
                Catch.Exception(() => fileBasedDataExportService.CreateExportStructureByTemplate(questionnaireExportStructure)) as InterviewDataExportException;

        It should_not_raise_exception = () =>
            raisedException.ShouldBeNull();

        It should_delete_exported_data_directory = () =>
            fileSystemAccessorMock.Verify(accessor => accessor.DeleteDirectory(it.Is<string>(name => name.Contains("ExportedData"))), Times.Once);

        It should_copy_exported_data_directory = () =>
            fileSystemAccessorMock.Verify(accessor => accessor.CopyFileOrDirectory(it.Is<string>(name => name.Contains("ExportedData")), it.IsAny<string>()), Times.Once);

        It should_action_file_be_created = () =>
          dataFileExportServiceMock.Verify(dataFileExportService => dataFileExportService.CreateHeaderForActionFile(Moq.It.IsAny<string>()), Times.Once);

        It should_delete_exported_files_directory = () =>
            fileSystemAccessorMock.Verify(accessor => accessor.DeleteDirectory(it.Is<string>(name => name.Contains("ExportedFiles"))), Times.Once);

        It should_copy_exported_files_directory = () =>
            fileSystemAccessorMock.Verify(accessor => accessor.CopyFileOrDirectory(it.Is<string>(name => name.Contains("ExportedFiles")), it.IsAny<string>()), Times.Once);

        It should_file_folder_for_template_be_created = () =>
          fileSystemAccessorMock.Verify(accessor => accessor.CreateDirectory(it.Is<string>(name => name.Contains("ExportedFiles\\exported_files_" + questionnaireExportStructure.QuestionnaireId.ToString()))), Times.Once);

        private static FileBasedDataExportService fileBasedDataExportService;
        private static InterviewDataExportException raisedException;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static Mock<IDataFileExportService> dataFileExportServiceMock;
        private static QuestionnaireExportStructure questionnaireExportStructure= new QuestionnaireExportStructure();
    }
}
