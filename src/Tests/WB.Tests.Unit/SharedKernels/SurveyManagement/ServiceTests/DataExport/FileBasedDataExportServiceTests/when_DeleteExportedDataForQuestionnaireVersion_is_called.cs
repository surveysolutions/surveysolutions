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

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.FileBasedDataExportServiceTests
{
    internal class when_DeleteExportedDataForQuestionnaireVersion_is_called : FileBasedDataExportServiceTestContext
    {
        Establish context = () =>
        {
            fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            filebaseExportDataAccessorMock = new Mock<IFilebasedExportedDataAccessor>();
            filebaseExportDataAccessorMock.Setup(x => x.GetFolderPathOfFilesByQuestionnaire(questionnaireId, questionnaireVersion))
                .Returns("binary files");
            filebaseExportDataAccessorMock.Setup(x => x.GetFolderPathOfDataByQuestionnaire(questionnaireId, questionnaireVersion))
                .Returns("data files");
            fileBasedDataExportRepositoryWriter = CreateFileBasedDataExportService(fileSystemAccessor: fileSystemAccessorMock.Object, filebasedExportedDataAccessor: filebaseExportDataAccessorMock.Object);
        };

        Because of = () => fileBasedDataExportRepositoryWriter.DeleteExportedDataForQuestionnaireVersion(questionnaireId, questionnaireVersion);

        It should_delete_data_folder = () =>
            fileSystemAccessorMock.Verify(x => x.DeleteDirectory("data files"), Times.Once());

        It should_delete_files_folder = () =>
            fileSystemAccessorMock.Verify(x => x.DeleteDirectory("binary files"), Times.Once());

        private static FileBasedDataExportRepositoryWriter fileBasedDataExportRepositoryWriter;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static Mock<IFilebasedExportedDataAccessor> filebaseExportDataAccessorMock;

        private static Guid questionnaireId = Guid.NewGuid();
        private static long questionnaireVersion = 3;
    }
}
