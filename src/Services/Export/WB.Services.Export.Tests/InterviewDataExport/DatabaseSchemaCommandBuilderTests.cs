using System.Collections.Generic;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using ApprovalTests.Reporters.TestFrameworks;
using NUnit.Framework;
using WB.Services.Export.InterviewDataStorage;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Tests.InterviewDataExport
{
    [UseApprovalSubdirectory("DatabaseSchemaCommandBuilderTests-approved")]
    [IgnoreLineEndings(true)]
    [UseReporter(typeof(DiffReporter), typeof(NUnitReporter))]
    [TestOf(typeof(DatabaseSchemaCommandBuilder))]
    public class DatabaseSchemaCommandBuilderTests
    {
        [Test]
        public void when_build_create_table_query()
        {
            var builder = new DatabaseSchemaCommandBuilder();

            List<CreateTableColumnInfo> columns = new List<CreateTableColumnInfo>()
            {
                new CreateTableColumnInfo("interview_id", InterviewDatabaseConstants.SqlTypes.Guid, isPrimaryKey: true),
                new CreateTableColumnInfo("roster_vector", InterviewDatabaseConstants.SqlTypes.IntArray, isPrimaryKey: true),
                new CreateTableColumnInfo("text_q", InterviewDatabaseConstants.SqlTypes.String, isNullable: true),
                new CreateTableColumnInfo("int_q", InterviewDatabaseConstants.SqlTypes.Integer, isNullable: true),
                new CreateTableColumnInfo("real_q", InterviewDatabaseConstants.SqlTypes.Double, isNullable: true),
                new CreateTableColumnInfo("text_list_q", InterviewDatabaseConstants.SqlTypes.Jsonb, isNullable: true),
                new CreateTableColumnInfo("single_q", InterviewDatabaseConstants.SqlTypes.Integer, isNullable: true),
                new CreateTableColumnInfo("multi_q", InterviewDatabaseConstants.SqlTypes.IntArray, isNullable: true),
                new CreateTableColumnInfo("audio_q", InterviewDatabaseConstants.SqlTypes.Jsonb, isNullable: true),
                new CreateTableColumnInfo("multimedia_q", InterviewDatabaseConstants.SqlTypes.Jsonb, isNullable: true),
            };

            var script = builder.GenerateCreateTableScript("test-table-name", columns);

            Approvals.Verify(script);
        }

        [Test]
        public void when_build_create_schema_query()
        {
            var builder = new DatabaseSchemaCommandBuilder();

            var script = builder.GenerateCreateSchema(new TenantInfo(string.Empty, "tenantid", "binary"));

            Approvals.Verify(script);
        }
    }
}
