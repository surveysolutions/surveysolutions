using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = System.Reflection.PortableExecutable.Machine.Specifications.It;

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

            var interviewStatuses = new TestInMemoryWriter<InterviewStatuses>();

            interviewStatuses.Store(
                Create.InterviewStatuses(questionnaireId: questionnaireId, questionnaireVersion: questionnaireVersion,
                    statuses: new[] {Create.InterviewCommentedStatus()}),
                "id");


            var questionnaireExportStructure = Create.QuestionnaireExportStructure();
            questionnaireExportStructure.HeaderToLevelMap.Add(new ValueVector<Guid>(), Create.HeaderStructureForLevel());
            sqlToTabDataExportService = CreateSqlToTabDataExportService(exportedDataAccessor: sqlDataAccessor.Object, csvWriterService:csvWriterServiceMock.Object,
                fileSystemAccessor: fileSystemAccessor.Object, interviewStatuses: interviewStatuses, questionnaireExportStructure: questionnaireExportStructure);
        };

        Because of = () =>
            filePaths = sqlToTabDataExportService.GetDataFilesForQuestionnaire(questionnaireId, questionnaireVersion, "");

        It should_return_one_element= () =>
            filePaths.Length.ShouldEqual(1);

        It should_return_correct_file_name = () =>
            filePaths[0].ShouldEqual(fileName);

        It should_record_one_completed_action = () =>
           csvWriterServiceMock.Verify(x=>x.WriteField("Completed"), Times.Once);

        private static SqlToDataExportService sqlToTabDataExportService;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long questionnaireVersion = 3;
        private static string[] filePaths;
        private static string fileName = "1.tab";
        private static Mock<ICsvWriterService> csvWriterServiceMock = new Mock<ICsvWriterService>();
    }
}
 