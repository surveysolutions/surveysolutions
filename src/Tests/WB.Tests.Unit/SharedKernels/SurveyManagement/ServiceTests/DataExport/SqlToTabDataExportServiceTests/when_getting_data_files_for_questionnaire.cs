using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.SqlToTabDataExportServiceTests
{
    internal class when_getting_data_files_for_questionnaire : SqlToTabDataExportServiceTestContext
    {
        Establish context = () =>
        {
            var sqlDataAccessor = new Mock<IExportedDataAccessor>();
            sqlDataAccessor.Setup(x => x.GetAllDataFolder(Moq.It.IsAny<string>())).Returns(string.Empty);

            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(false);
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(Moq.It.IsAny<string>())).Returns(new[] { fileName, "2.txt" });

            sqlToTabDataExportService = CreateSqlToTabDataExportService(exportedDataAccessor: sqlDataAccessor.Object, 
                fileSystemAccessor: fileSystemAccessor.Object);
        };

        Because of = () =>
            filePaths = sqlToTabDataExportService.GetDataFilesForQuestionnaire(questionnaireId, questionnaireVersion, "");

        private It should_return_one_element= () =>
            filePaths.Length.ShouldEqual(1);

        private It should_return_correct_file_name = () =>
            filePaths[0].ShouldEqual(fileName);

        private static SqlToTabDataExportService sqlToTabDataExportService;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long questionnaireVersion = 3;
        private static string[] filePaths;
        private static string fileName = "1.tab";
    }
}
 