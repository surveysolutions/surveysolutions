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

        It should_7_commands_be_executed = () =>
             sqlServiceTestable.CommandsToExecute.Count.ShouldEqual(7);

        It should_first_command_be_select_of_all_tables_in_database = () =>
            sqlServiceTestable.CommandsToExecute[0].ShouldEqual("SELECT name FROM sqlite_master WHERE type='table'");

        It should_second_command_be_select_of_all_columns_for_interview_action_table = () =>
           sqlServiceTestable.CommandsToExecute[1].ShouldEqual("PRAGMA table_info('interview_actions')");

        It should_third_command_be_delete_from_interview_actions_table_by_id = () =>
           sqlServiceTestable.CommandsToExecute[2].ShouldEqual("DELETE FROM [interview_actions] WHERE [Id] = @interviewId;");

        It should_fourth_command_be_select_of_all_columns_for_roster_table = () =>
            sqlServiceTestable.CommandsToExecute[3].ShouldEqual("PRAGMA table_info('main')");

        It should_fifth_command_be_delete_from_main_table_by_ParentId2 = () =>
            sqlServiceTestable.CommandsToExecute[4].ShouldEqual("DELETE FROM [main] WHERE [Id] = @interviewId;");

        It should_six_command_be_select_of_all_columns_for_main_table = () =>
            sqlServiceTestable.CommandsToExecute[5].ShouldEqual("PRAGMA table_info('roster')");

        It should_seventh_command_be_delete_from_roster_table_by_id = () =>
            sqlServiceTestable.CommandsToExecute[6].ShouldEqual("DELETE FROM [roster] WHERE [ParentId2] = @interviewId;");

        private static SqlDataExportWriter sqlDataExportWriter;
        private static SqlServiceTestable sqlServiceTestable;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
    }
}
