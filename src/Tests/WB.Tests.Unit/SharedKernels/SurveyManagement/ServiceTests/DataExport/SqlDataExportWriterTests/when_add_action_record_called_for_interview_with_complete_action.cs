using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.SqlDataExportWriterTests
{
    internal class when_add_action_record_called_for_interview_with_complete_action : SqlDataExportWriterTestContext
    {
        Establish context = () =>
        {
            sqlServiceTestable = new SqlServiceTestable();
            fileSystemAccessorMock = CreateIFileSystemAccessorMock();

            interviewActionExportView = new InterviewActionExportView(interviewId, InterviewExportedAction.Completed,
                "nastya", dataTime, "inter");

            sqlDataExportWriter = CreateSqlDataExportWriter(sqlService: sqlServiceTestable,
                fileSystemAccessor: fileSystemAccessorMock.Object);
        };

        Because of = () =>
            sqlDataExportWriter.AddActionRecord(interviewActionExportView, "");

        It should_1_command_be_executed = () =>
             sqlServiceTestable.CommandsToExecute.Count.ShouldEqual(1);

        It should_command_with_action_insert_be_executed = () =>
            sqlServiceTestable.CommandsToExecute[0].ShouldEqual("insert into [interview_actions] values (@var1,@var2,@var3,@var4,@var5,@var6);");

        It should_one_folder_be_deleted = () =>
            fileSystemAccessorMock.Verify(x=>x.DeleteDirectory(Moq.It.IsAny<string>()), Times.Once);

        It should_one_folder_with_all_data_be_deleted = () =>
            fileSystemAccessorMock.Verify(x => x.DeleteDirectory(Moq.It.Is<string>(s=>s.Contains("AllData"))), Times.Once);

        private static SqlDataExportWriter sqlDataExportWriter;
        private static SqlServiceTestable sqlServiceTestable;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static InterviewActionExportView interviewActionExportView;
        private static string interviewId = "interviewId1";
        private static DateTime dataTime = new DateTime(1984, 4, 18, 6, 38, 2);
    }
}
