using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.ServiceTests.DataExport.SqlDataExportWriterTests
{
    internal class when_delete_interview_method_is_called : SqlDataExportWriterTestContext
    {
        Establish context = () =>
        {
            sqlServiceTestable = new SqlServiceTestable(new Dictionary<string, string[]> { { "interview_actions", new[] { "Id" } }, { "main", new[] { "Id" } }, { "roster", new[] { "Id", "ParentId1", "ParentId2" } } });

            sqlDataExportWriter = CreateSqlDataExportWriter(sqlService: sqlServiceTestable);
        };

        Because of = () =>
            sqlDataExportWriter.DeleteInterviewRecords("",interviewId);

        It should_4_commands_be_executed = () =>
             sqlServiceTestable.CommandsToExecute.Count.ShouldEqual(4);

        It should_first_command_be_select_of_all_tables_in_database = () =>
            sqlServiceTestable.CommandsToExecute[0].ShouldEqual("SELECT name FROM sqlite_master WHERE type='table';");

        It should_second_command_be_delete_from_interview_actions_table_by_id = () =>
           sqlServiceTestable.CommandsToExecute[1].ShouldEqual("DELETE FROM [interview_actions] WHERE [InterviewId] = @interviewId;");

        It should_third_command_be_delete_from_main_table_by_ParentId2 = () =>
            sqlServiceTestable.CommandsToExecute[2].ShouldEqual("DELETE FROM [main] WHERE [InterviewId] = @interviewId;");

        It should_fourth_command_be_delete_from_roster_table_by_id = () =>
            sqlServiceTestable.CommandsToExecute[3].ShouldEqual("DELETE FROM [roster] WHERE [InterviewId] = @interviewId;");

        private static SqlDataExportWriter sqlDataExportWriter;
        private static SqlServiceTestable sqlServiceTestable;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
    }
}
