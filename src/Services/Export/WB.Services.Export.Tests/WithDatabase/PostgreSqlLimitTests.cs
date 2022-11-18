using System;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApprovalUtilities.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NpgsqlTypes;
using NUnit.Framework;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.InterviewDataStorage;
using WB.Services.Export.InterviewDataStorage.InterviewDataExport;
using WB.Services.Export.InterviewDataStorage.Services;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure;
using WB.Services.Infrastructure.EventSourcing;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Tests.WithDatabase;

public class PostgreSqlLimitTests
{
    [Test]
    public async Task try_create_table_with_1000_string()
    {
        var connectionOptions = Options.Create(new DbConnectionSettings
        {
            DefaultConnection = TestConfig.GetConnectionString()
        });

        var tenant = new TenantInfo("http://example", Guid.NewGuid().FormatGuid(), "testTenant");

        var ctx = new TenantContext(null, tenant);

        var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
        optionsBuilder.UseNpgsql(connectionOptions.Value.DefaultConnection,
            b => { b.MigrationsHistoryTable("__migrations", tenant.SchemaName());});
        var db = new TenantDbContext(ctx, connectionOptions, optionsBuilder.Options, Mock.Of<ILogger<TenantDbContext>>());
        var generator = new QuestionnaireSchemaGenerator(ctx, db, new DatabaseSchemaCommandBuilder(),
            new NullLogger<DatabaseSchemaService>());

        var groups = Enumerable.Range(1, 10).Select(i =>
            Create.Group(Guid.NewGuid(), "group #" + i, 
                children: Enumerable.Range(1, 100).Select(ci =>
                    Create.TextQuestion(Guid.NewGuid(), "title #" + i + ci, "var" + i + ci)
                ).Cast<IQuestionnaireEntity>().ToArray())
        ).Cast<IQuestionnaireEntity>().ToArray();
        var questionnaire = Create.QuestionnaireDocument(Guid.NewGuid(), children: groups);
        
        await using (var tr = await db.Database.BeginTransactionAsync())
        {
            generator.CreateQuestionnaireDbStructure(questionnaire);
            await tr.CommitAsync();
        }

        var state = new InterviewDataState();
        var interviewId = Guid.NewGuid();
        var levelTables = questionnaire.DatabaseStructure.GetLevelTables(questionnaire.PublicKey);
        foreach (var levelTable in levelTables)
        {
            state.InsertInterviewInTable(levelTable.TableName, interviewId);
        }

        var commandBuilder = new InterviewDataExportBulkCommandBuilder(new InterviewDataExportBulkCommandBuilderSettings());
        groups.SelectMany(g => g.Children).Cast<TextQuestion>().ForEach(tq =>
        {
            var tableName = questionnaire.DatabaseStructure.GetDataTableName(tq.PublicKey);
            state.UpdateValueInTable(tableName, interviewId, new RosterVector(), tq.ColumnName, "12345678901234567890123", NpgsqlDbType.Text);
        });

        var dbCommand = commandBuilder.BuildCommandsInExecuteOrderFromState(state);
        
        await using (var tr = await db.Database.BeginTransactionAsync())
        {
            await new CommandExecutor(db).ExecuteNonQueryAsync(dbCommand, CancellationToken.None);
            await tr.CommitAsync();
        }
    }
}