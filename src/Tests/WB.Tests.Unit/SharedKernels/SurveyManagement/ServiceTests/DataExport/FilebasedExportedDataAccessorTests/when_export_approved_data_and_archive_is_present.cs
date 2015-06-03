using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.FilebasedExportedDataAccessorTests
{
    internal class when_export_approved_data_and_archive_is_present : FilebasedExportedDataAccessorTestContext
    {
        Establish context = () =>
        {
            fileSystemAccessorMock = CreateFileSystemAccessorMock();
            fileSystemAccessorMock.Setup(
                x => x.IsFileExists(Moq.It.IsAny<string>()))
                .Returns(true);

            filebasedExportedDataAccessor = CreateFilebasedExportedDataAccessor(
                fileSystemAccessor: fileSystemAccessorMock.Object,
                dataFiles: new[] { "f1", "f2" },
                environmentFiles: new[] { "e1", "e2" },
                zipCallback: (f, d) => addedFiles = f.ToArray());
        };

        Because of = () =>
            archiveName = filebasedExportedDataAccessor.GetFilePathToExportedApprovedCompressedData(questionnaireId, questionnaireVersion, ExportDataType.Tab);

        It should_archive_name_contain_questionnaire_id_and_version_and_Approved_addition = () =>
            archiveName.ShouldContain("exported_data_11111111-1111-1111-1111-111111111111_3_Tab_Approved");

        It should_archive_contain_data_files_and_environment_files = () =>
            addedFiles.ShouldEqual(new[] { "f1", "f2", "e1", "e2" });

        It should_existing_archive_be_deleted = () =>
          fileSystemAccessorMock.Verify(x => x.DeleteFile(Moq.It.IsAny<string>()), Times.Once);

        private static FilebasedExportedDataAccessor filebasedExportedDataAccessor;

        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long questionnaireVersion = 3;

        private static string[] addedFiles;
        private static string archiveName;

        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
    }
}
