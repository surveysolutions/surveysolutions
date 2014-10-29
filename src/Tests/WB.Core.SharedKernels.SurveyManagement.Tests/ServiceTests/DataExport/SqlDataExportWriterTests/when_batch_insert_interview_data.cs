using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.ServiceTests.DataExport.SqlDataExportWriterTests
{
    internal class when_batch_insert_interview_data : SqlDataExportWriterTestContext
    {
        Establish context = () =>
        {
            sqlServiceTestable = new SqlServiceTestable(new Dictionary<string, string[]> { { "interview_actions", new[] { "Id" } }, { "roster level table", new[] { "Id", "ParentId1", "ParentId2" } } });
            interviewDataExportView = CreateInterviewDataExportView(interviewId: interviewId,
                levels: CreateInterviewDataExportLevelView(interviewId: interviewId, levelName: "roster level table",
                    levelVector: new ValueVector<Guid>(new[] { Guid.NewGuid(), Guid.NewGuid() }),
                    records: new[]
                    {
                        CreateInterviewDataExportRecord(interviewId, "0", new[] { "r1", "r2" }, new[] { "1", interviewId.FormatGuid() }),
                        CreateInterviewDataExportRecord(interviewId, "1", new[] { "r3", "r4" }, new[] { "2", interviewId.FormatGuid() })
                    }));
            sqlDataExportWriter = CreateSqlDataExportWriter(sqlService: sqlServiceTestable);
        };

        Because of = () =>
            sqlDataExportWriter.BatchInsert("", new[] { interviewDataExportView }, new[] { new InterviewActionExportView(interviewId.FormatGuid(), InterviewExportedAction.ApproveByHeadquarter, "nastya", DateTime.Now, "int") }, new[] { interviewIdForDelete });

        It should_9_commands_be_executed = () =>
             sqlServiceTestable.CommandsToExecute.Count.ShouldEqual(9);

        It should_one_action_be_inserted = () =>
            sqlServiceTestable.CommandsToExecute[0].ShouldEqual("insert into [interview_actions] values (@var1,@var2,@var3,@var4,@var5,@var6);");

        It should_forth_command_be_delete_from_Action_table = () =>
           sqlServiceTestable.CommandsToExecute[3].ShouldEqual("DELETE FROM [interview_actions] WHERE [Id] = @interviewId;");

        It should_seventh_command_be_delete_from_roster_table = () =>
          sqlServiceTestable.CommandsToExecute[6].ShouldEqual("DELETE FROM [roster level table] WHERE [ParentId2] = @interviewId;");

        It should_eight_command_be_insert_first_new_interview_data_in_level_table = () =>
           sqlServiceTestable.CommandsToExecute[7].ShouldEqual("insert into [roster level table] values ('0','r1','r2','a','1','1','11111111111111111111111111111111');");

        It should_nine_command_be_insert_second_new_interview_data_in_level_table = () =>
            sqlServiceTestable.CommandsToExecute[8].ShouldEqual("insert into [roster level table] values ('1','r3','r4','a','1','2','11111111111111111111111111111111');");

        private static SqlDataExportWriter sqlDataExportWriter;
        private static SqlServiceTestable sqlServiceTestable;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static Guid interviewIdForDelete = Guid.Parse("22222222222222222222222222222222");
        private static InterviewDataExportView interviewDataExportView;
    }
}
