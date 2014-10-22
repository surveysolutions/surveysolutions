using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.ServiceTests.DataExport.SqlDataExportWriterTests
{
    internal class when_create_interview_action_header_is_called_for_interview : SqlDataExportWriterTestContext
    {
        Establish context = () =>
        {
            sqlServiceTestable = new SqlServiceTestable();
            sqlDataExportWriter = CreateSqlDataExportWriter(sqlServiceTestable);
        };

        Because of = () =>
            sqlDataExportWriter.CreateHeaderForActionFile("");

        It should_1_command_be_executed = () =>
             sqlServiceTestable.CommandsToExecute.Count.ShouldEqual(1);

        It should_command_with_level_table_creation_be_executed = () =>
            sqlServiceTestable.CommandsToExecute[0].ShouldEqual("create table interview_actions (Id nvarchar(512), Action nvarchar(512), Originator nvarchar(512), Role nvarchar(512), Date nvarchar(512), Time nvarchar(512));");

        private static SqlDataExportWriter sqlDataExportWriter;
        private static SqlServiceTestable sqlServiceTestable;
    }
}
