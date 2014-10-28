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
    internal class when_create_header_is_called_for_main_level_of_the_interview : SqlDataExportWriterTestContext
    {
        Establish context = () =>
        {
            sqlServiceTestable=new SqlServiceTestable();
            questionnaireExportStructure = CreateQuestionnaireExportStructure(CreateHeaderStructureForLevel(levelName: "my table"));
            sqlDataExportWriter = CreateSqlDataExportWriter(sqlServiceTestable);
        };

        Because of = () =>
            sqlDataExportWriter.CreateStructure(questionnaireExportStructure, "");

        It should_3_commands_be_executed = () =>
            sqlServiceTestable.CommandsToExecute.Count.ShouldEqual(3);

        It should_command_with_level_table_creation_be_executed = () =>
            sqlServiceTestable.CommandsToExecute[0].ShouldEqual("create table [my table] ([Id] nvarchar(512),[1] ntext,[a] money);");

        It should_command_with_index_creation_for_level_table_be_executed = () =>
            sqlServiceTestable.CommandsToExecute[1].ShouldEqual("CREATE INDEX [idxmy table] ON [my table]([Id]);");

        private static SqlDataExportWriter sqlDataExportWriter;
        private static SqlServiceTestable sqlServiceTestable;
        private static QuestionnaireExportStructure questionnaireExportStructure;
    }
}
