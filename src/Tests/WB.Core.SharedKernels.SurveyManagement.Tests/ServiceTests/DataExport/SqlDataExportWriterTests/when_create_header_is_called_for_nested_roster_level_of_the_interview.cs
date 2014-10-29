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
    internal class when_create_header_is_called_for_nested_roster_level_of_the_interview : SqlDataExportWriterTestContext
    {
        Establish context = () =>
        {
            sqlServiceTestable = new SqlServiceTestable();
            questionnaireExportStructure =CreateQuestionnaireExportStructure( CreateHeaderStructureForLevel(levelName: "my_table",
                levelScopeVector: new ValueVector<Guid>(new[] { Guid.NewGuid(), Guid.NewGuid() }),referenceNames:new []{"ref1","ref2"}));
            sqlDataExportWriter = CreateSqlDataExportWriter(sqlServiceTestable);
        };

        Because of = () =>
            sqlDataExportWriter.CreateStructure(questionnaireExportStructure, "");

        It should_command_with_level_table_creation_be_executed = () =>
            sqlServiceTestable.CommandsToExecute[0].ShouldEqual("create table [my_table] ([Id] TEXT,[ref1] TEXT,[ref2] TEXT,[1] TEXT,[a] TEXT,[ParentId1] TEXT,[ParentId2] NVARCHAR(128));");

        It should_command_with_index_creation_for_level_table_be_executed = () =>
            sqlServiceTestable.CommandsToExecute[1].ShouldEqual("CREATE INDEX [idxmy_table] ON [my_table]([ParentId2]);");

        private static SqlDataExportWriter sqlDataExportWriter;
        private static SqlServiceTestable sqlServiceTestable;
        private static QuestionnaireExportStructure questionnaireExportStructure;

    }
}
