using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.SqlToTabDataExportServiceTests
{
    internal class when_getting_approved_data_files_for_questionnaire : SqlToTabDataExportServiceTestContext
    {
        Establish context = () =>
        {
            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(false);
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(Moq.It.IsAny<string>())).Returns(new[] { fileName, "2.txt" });
            var approvedInterviewId = Guid.NewGuid();
            var interviewStatuses = new TestInMemoryWriter<InterviewStatuses>();

            interviewStatuses.Store(
                Create.InterviewStatuses(questionnaireId: questionnaireId, questionnaireVersion: questionnaireVersion,
                    statuses:
                        new[]
                        {
                            Create.InterviewCommentedStatus()
                        }),
                "id1");

            interviewStatuses.Store(
                Create.InterviewStatuses(questionnaireId: questionnaireId, questionnaireVersion: questionnaireVersion,
                    interviewid: approvedInterviewId,
                    statuses:
                        new[]
                        {
                            Create.InterviewCommentedStatus(),
                            Create.InterviewCommentedStatus(status: InterviewExportedAction.ApprovedByHeadquarter)
                        }),
                "id2");

            var questionnaireExportStructure = Create.QuestionnaireExportStructure();
            questionnaireExportStructure.HeaderToLevelMap.Add(new ValueVector<Guid>(), Create.HeaderStructureForLevel());
            sqlToTabDataExportService = CreateSqlToTabDataExportService(csvWriterService: csvWriterServiceMock.Object,
                fileSystemAccessor: fileSystemAccessor.Object, interviewStatuses: interviewStatuses, questionnaireExportStructure: questionnaireExportStructure);
        };

        Because of = () =>
            filePaths = sqlToTabDataExportService.GetDataFilesForQuestionnaireByInterviewsInApprovedState(questionnaireId, questionnaireVersion, "");

        It should_return_one_element = () =>
            filePaths.Length.ShouldEqual(1);

        It should_return_correct_file_name = () =>
            filePaths[0].ShouldEqual(fileName);

        It should_record_one_completed_action = () =>
           csvWriterServiceMock.Verify(x => x.WriteField("Completed"), Times.Once);

        It should_record_one_approved_by_hq_action = () =>
        csvWriterServiceMock.Verify(x => x.WriteField("ApprovedByHeadquarter"), Times.Once);

        private static SqlToDataExportService sqlToTabDataExportService;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long questionnaireVersion = 3;
        private static string[] filePaths;
        private static string fileName = "1.tab";
        private static Mock<ICsvWriterService> csvWriterServiceMock = new Mock<ICsvWriterService>();
    }
}