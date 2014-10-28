using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.ServiceTests.DataExport.FileBasedDataExportServiceTests
{
    internal class when_DeleteExportedDataForQuestionnaireVersion_is_called : FileBasedDataExportServiceTestContext
    {
        Establish context = () =>
        {
            fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            filebaseExportDataAccessorMock = new Mock<IFilebaseExportDataAccessor>();
            filebaseExportDataAccessorMock.Setup(x => x.GetFolderPathOfFilesByQuestionnaireOrThrow(questionnaireId, questionnaireVersion))
                .Returns("binary files");
            filebaseExportDataAccessorMock.Setup(x => x.GetFolderPathOfDataByQuestionnaireOrThrow(questionnaireId, questionnaireVersion))
                .Returns("data files");
            fileBasedDataExportRepositoryWriter = CreateFileBasedDataExportService(fileSystemAccessor: fileSystemAccessorMock.Object, filebaseExportDataAccessor: filebaseExportDataAccessorMock.Object);
        };

        Because of = () => fileBasedDataExportRepositoryWriter.DeleteExportedDataForQuestionnaireVersion(questionnaireId, questionnaireVersion);

        It should_data_folder_be_deleted = () =>
            fileSystemAccessorMock.Verify(x => x.DeleteDirectory("data files"), Times.Once());

        It should_files_folder_be_deleted = () =>
            fileSystemAccessorMock.Verify(x => x.DeleteDirectory("binary files"), Times.Once());

        private static FileBasedDataExportRepositoryWriter fileBasedDataExportRepositoryWriter;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static Mock<IFilebaseExportDataAccessor> filebaseExportDataAccessorMock;

        private static Guid questionnaireId = Guid.NewGuid();
        private static long questionnaireVersion = 3;
    }
}
