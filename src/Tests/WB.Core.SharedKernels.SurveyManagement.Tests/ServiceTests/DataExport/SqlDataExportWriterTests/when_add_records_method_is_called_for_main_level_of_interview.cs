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
    internal class when_add_records_method_is_called_for_main_level_of_interview : SqlDataExportWriterTestContext
    {
        Establish context = () =>
        {
            sqlServiceTestable = new SqlServiceTestable();
            interviewDataExportLevelView = CreateInterviewDataExportLevelView(interviewId, "main level table");
            sqlDataExportWriter = CreateSqlDataExportWriter(sqlService: sqlServiceTestable);
        };

        Because of = () =>
            sqlDataExportWriter.AddRecords(interviewDataExportLevelView,"");

        It should_2_commands_be_executed = () =>
             sqlServiceTestable.CommandsToExecute.Count.ShouldEqual(2);

        It should_first_command_be_delete_all_interview_data_by_id = () =>
            sqlServiceTestable.CommandsToExecute[0].ShouldEqual("DELETE FROM \"main level table\" WHERE Id = '11111111111111111111111111111111';");

        It should_second_command_be_intert_new_interview_data_in_level_table = () =>
           sqlServiceTestable.CommandsToExecute[1].ShouldEqual("insert into \"main level table\" values ('11111111111111111111111111111111','a',1);");

        private static SqlDataExportWriter sqlDataExportWriter;
        private static SqlServiceTestable sqlServiceTestable;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static DateTime dataTime = new DateTime(1984, 4, 18, 6, 38, 2);
        private static InterviewDataExportLevelView interviewDataExportLevelView;
    }
}
