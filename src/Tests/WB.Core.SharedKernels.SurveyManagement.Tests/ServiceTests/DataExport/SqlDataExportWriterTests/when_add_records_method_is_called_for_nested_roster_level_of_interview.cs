using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.ServiceTests.DataExport.SqlDataExportWriterTests
{
    internal class when_add_records_method_is_called_for_nested_roster_level_of_interview : SqlDataExportWriterTestContext
    {
        Establish context = () =>
        {
            sqlServiceTestable = new SqlServiceTestable();
            interviewDataExportView = CreateInterviewDataExportView(interviewId: interviewId,
                levels: CreateInterviewDataExportLevelView(interviewId: interviewId, levelName: "main level table",
                    levelVector: new ValueVector<Guid>(new[] { Guid.NewGuid(), Guid.NewGuid() }),
                    records: new[]
                    {
                        CreateInterviewDataExportRecord(interviewId, "0", new[] { "r1", "r2" }, new[] { "1", "parentIdTest1" }),
                        CreateInterviewDataExportRecord(interviewId, "1", new[] { "r3", "r4" }, new[] { "2", "parentIdTest2" })
                    }));
            sqlDataExportWriter = CreateSqlDataExportWriter(sqlService: sqlServiceTestable);
        };

        Because of = () =>
            sqlDataExportWriter.AddOrUpdateInterviewRecords(interviewDataExportView, "");

        It should_3_commands_be_executed = () =>
             sqlServiceTestable.CommandsToExecute.Count.ShouldEqual(3);

        It should_first_command_be_delete_all_interview_data_by_id = () =>
            sqlServiceTestable.CommandsToExecute[0].ShouldEqual("DELETE FROM [main level table] WHERE [ParentId2] = @interviewId;");

        It should_second_command_be_intert_first_new_interview_data_in_level_table = () =>
           sqlServiceTestable.CommandsToExecute[1].ShouldEqual("insert into [main level table] values ('0','r1','r2','a','1','1','parentIdTest1');");

        It should_third_command_be_intert_second_new_interview_data_in_level_table = () =>
          sqlServiceTestable.CommandsToExecute[2].ShouldEqual("insert into [main level table] values ('1','r3','r4','a','1','2','parentIdTest2');");

        private static SqlDataExportWriter sqlDataExportWriter;
        private static SqlServiceTestable sqlServiceTestable;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static InterviewDataExportView interviewDataExportView;
    }
}
