using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.InterviewDataStorage.InterviewDataExport;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.InterviewDataStorage.Services
{
    public class QuestionnaireSchemaGenerator : IQuestionnaireSchemaGenerator
    {
        private readonly ITenantContext tenantContext;
        private readonly TenantDbContext dbContext;
        private readonly IDatabaseSchemaCommandBuilder commandBuilder;
        private readonly ILogger<DatabaseSchemaService> logger;
        private readonly IOptions<DbConnectionSettings> connectionSettings;

        public QuestionnaireSchemaGenerator(ITenantContext tenantContext,
            TenantDbContext dbContext,
            IDatabaseSchemaCommandBuilder commandBuilder,
            ILogger<DatabaseSchemaService> logger,
            IOptions<DbConnectionSettings> connectionSettings)
        {
            this.tenantContext = tenantContext;
            this.dbContext = dbContext;
            this.commandBuilder = commandBuilder;
            this.logger = logger;
            this.connectionSettings = connectionSettings;
        }

        public void CreateQuestionnaireDbStructure(QuestionnaireDocument questionnaireDocument)
        {
            try
            {
                var connection = dbContext.Database.GetDbConnection();

                CreateSchema(connection, tenantContext.Tenant);

                foreach (var storedGroup in questionnaireDocument.DatabaseStructure.GetAllLevelTables())
                {
                    CreateTableForGroup(connection, storedGroup, questionnaireDocument);
                    CreateEnablementTableForGroup(connection, storedGroup);
                    CreateValidityTableForGroup(connection, storedGroup);
                }

                dbContext.SaveChanges();

                logger.LogInformation("Created database structure for {tenantName} ({questionnaireId} [{table}])",
                    this.tenantContext.Tenant?.Name, questionnaireDocument.QuestionnaireId,
                    questionnaireDocument.TableName);
            }
            catch (Exception e)
            {
                e.Data.Add("questionnaireId", questionnaireDocument.QuestionnaireId.ToString());
                e.Data.Add("questionnaireTitle", questionnaireDocument.Title);
                throw e;
            }
        }

        public void DropQuestionnaireDbStructure(QuestionnaireDocument questionnaireDocument)
        {
            try
            {
                var db = dbContext.Database.GetDbConnection();
                foreach (var storedGroup in questionnaireDocument.DatabaseStructure.GetAllLevelTables())
                {
                    db.Execute(commandBuilder.GenerateDropTable(storedGroup.TableName)
                               + commandBuilder.GenerateDropTable(storedGroup.EnablementTableName)
                               + commandBuilder.GenerateDropTable(storedGroup.ValidityTableName));
                }

                logger.LogDebug("Skipping questionnaire creation for {tenantName} ({questionnaireId}[{table}])",
                    this.tenantContext.Tenant?.Name, questionnaireDocument.QuestionnaireId, questionnaireDocument.TableName);
            }
            catch (Exception e)
            {
                e.Data.Add("questionnaireId", questionnaireDocument.QuestionnaireId.ToString());
                e.Data.Add("questionnaireTitle", questionnaireDocument.Title);
                throw e;
            }
        }

        private void CreateTableForGroup(DbConnection connection, QuestionnaireLevelDatabaseTable group, QuestionnaireDocument questionnaireDocument)
        {
            var columns = new List<CreateTableColumnInfo>();
            columns.Add(new CreateTableColumnInfo(InterviewDatabaseConstants.InterviewId, InterviewDatabaseConstants.SqlTypes.Guid, isPrimaryKey: true));
            if (group.IsRoster)
                columns.Add(new CreateTableColumnInfo(InterviewDatabaseConstants.RosterVector, $"int4[]", isPrimaryKey: true));

            var questions = group.DataColumns.Where(entity => entity is Question).Cast<Question>();
            foreach (var question in questions)
                columns.Add(new CreateTableColumnInfo(question.ColumnName, GetSqlTypeForQuestion(question, questionnaireDocument), isNullable: true));

            var variables = group.DataColumns.Where(entity => entity is Variable).Cast<Variable>();
            foreach (var variable in variables)
                columns.Add(new CreateTableColumnInfo(variable.ColumnName, GetSqlTypeForVariable(variable), isNullable: true));

            var commandText = commandBuilder.GenerateCreateTableScript(group.TableName, columns);
            connection.Execute(commandText);
        }

        private void CreateEnablementTableForGroup(DbConnection connection, QuestionnaireLevelDatabaseTable group)
        {
            var columns = new List<CreateTableColumnInfo>();
            columns.Add(new CreateTableColumnInfo(InterviewDatabaseConstants.InterviewId, InterviewDatabaseConstants.SqlTypes.Guid, isPrimaryKey: true));
            if (group.IsRoster)
                columns.Add(new CreateTableColumnInfo(InterviewDatabaseConstants.RosterVector, $"int4[]", isPrimaryKey: true));

            var questions = group.EnablementColumns.Where(entity => entity is Question).Cast<Question>();
            foreach (var question in questions)
                columns.Add(new CreateTableColumnInfo(question.ColumnName, InterviewDatabaseConstants.SqlTypes.Bool, isNullable: false, defaultValue: "true"));

            var variables = group.EnablementColumns.Where(entity => entity is Variable).Cast<Variable>();
            foreach (var variable in variables)
                columns.Add(new CreateTableColumnInfo(variable.ColumnName, InterviewDatabaseConstants.SqlTypes.Bool, isNullable: false, defaultValue: "true"));

            var staticTexts = group.EnablementColumns.Where(entity => entity is StaticText).Cast<StaticText>();
            foreach (var staticText in staticTexts)
                columns.Add(new CreateTableColumnInfo(staticText.ColumnName, InterviewDatabaseConstants.SqlTypes.Bool, isNullable: false, defaultValue: "true"));

            var nonRosters = group.EnablementColumns.Where(entity => entity is Group).Cast<Group>();
            foreach (var notRoster in nonRosters)
                columns.Add(new CreateTableColumnInfo(notRoster.ColumnName, InterviewDatabaseConstants.SqlTypes.Bool, isNullable: false, defaultValue: "true"));

            var commandText = commandBuilder.GenerateCreateTableScript(group.EnablementTableName, columns);
            connection.Execute(commandText);
        }

        private void CreateValidityTableForGroup(DbConnection connection, QuestionnaireLevelDatabaseTable group)
        {
            var columns = new List<CreateTableColumnInfo>();
            columns.Add(new CreateTableColumnInfo(InterviewDatabaseConstants.InterviewId, InterviewDatabaseConstants.SqlTypes.Guid, isPrimaryKey: true));
            if (group.IsRoster)
                columns.Add(new CreateTableColumnInfo(InterviewDatabaseConstants.RosterVector, $"int4[]", isPrimaryKey: true));

            var questions = group.ValidityColumns.Where(entity => entity is Question).Cast<Question>();
            foreach (var question in questions)
                columns.Add(new CreateTableColumnInfo(question.ColumnName, InterviewDatabaseConstants.SqlTypes.IntArray, isNullable: true));

            var staticTexts = group.ValidityColumns.Where(entity => entity is StaticText).Cast<StaticText>();
            foreach (var staticText in staticTexts)
                columns.Add(new CreateTableColumnInfo(staticText.ColumnName, InterviewDatabaseConstants.SqlTypes.IntArray, isNullable: true));

            var commandText = commandBuilder.GenerateCreateTableScript(group.ValidityTableName, columns);
            connection.Execute(commandText);
        }

        private string GetSqlTypeForQuestion(Question question, QuestionnaireDocument questionnaire)
        {
            switch (question)
            {
                case TextQuestion _: return InterviewDatabaseConstants.SqlTypes.String;
                case NumericQuestion numericQuestion when (numericQuestion.IsInteger): return InterviewDatabaseConstants.SqlTypes.Integer;
                case NumericQuestion numericQuestion when (!numericQuestion.IsInteger): return InterviewDatabaseConstants.SqlTypes.Double;
                case TextListQuestion _: return InterviewDatabaseConstants.SqlTypes.Jsonb;
                case MultimediaQuestion _: return InterviewDatabaseConstants.SqlTypes.String;
                case DateTimeQuestion dateTimeQuestion: return InterviewDatabaseConstants.SqlTypes.DateTime;
                case AudioQuestion audioQuestion: return InterviewDatabaseConstants.SqlTypes.Jsonb;
                case AreaQuestion areaQuestion: return InterviewDatabaseConstants.SqlTypes.Jsonb;
                case GpsCoordinateQuestion gpsCoordinateQuestion: return InterviewDatabaseConstants.SqlTypes.Jsonb;
                case QRBarcodeQuestion qrBarcodeQuestion: return InterviewDatabaseConstants.SqlTypes.String;
                case SingleQuestion singleQuestion when (singleQuestion.LinkedToRosterId.HasValue):
                    return InterviewDatabaseConstants.SqlTypes.IntArray;
                case SingleQuestion singleQuestion when (singleQuestion.LinkedToQuestionId.HasValue):
                    var singleSourceQuestion = questionnaire.Find<Question>(singleQuestion.LinkedToQuestionId.Value);
                    return singleSourceQuestion is TextListQuestion ? InterviewDatabaseConstants.SqlTypes.Integer : InterviewDatabaseConstants.SqlTypes.IntArray;
                case SingleQuestion singleQuestion
                    when (!singleQuestion.LinkedToQuestionId.HasValue && !singleQuestion.LinkedToRosterId.HasValue):
                    return InterviewDatabaseConstants.SqlTypes.Integer;
                case MultyOptionsQuestion yesNoOptionsQuestion when (yesNoOptionsQuestion.YesNoView):
                case MultyOptionsQuestion multiLinkedToRoster when (multiLinkedToRoster.LinkedToRosterId.HasValue):
                    return InterviewDatabaseConstants.SqlTypes.Jsonb;
                case MultyOptionsQuestion multiLinkedToQuestion when (multiLinkedToQuestion.LinkedToQuestionId.HasValue):
                    var multiSourceQuestion = questionnaire.Find<Question>(multiLinkedToQuestion.LinkedToQuestionId.Value);
                    return multiSourceQuestion is TextListQuestion ? InterviewDatabaseConstants.SqlTypes.IntArray : InterviewDatabaseConstants.SqlTypes.Jsonb;
                case MultyOptionsQuestion multiOptionsQuestion
                    when (!multiOptionsQuestion.LinkedToQuestionId.HasValue && !multiOptionsQuestion.LinkedToRosterId.HasValue):
                    return InterviewDatabaseConstants.SqlTypes.IntArray;
                default:
                    throw new ArgumentException("Unknown question type: " + question.GetType().Name);
            }
        }

        private string GetSqlTypeForVariable(Variable variable)
        {
            switch (variable.Type)
            {
                case VariableType.Boolean: return InterviewDatabaseConstants.SqlTypes.Bool;
                case VariableType.DateTime: return InterviewDatabaseConstants.SqlTypes.DateTime;
                case VariableType.Double: return InterviewDatabaseConstants.SqlTypes.Double;
                case VariableType.LongInteger: return InterviewDatabaseConstants.SqlTypes.Long;
                case VariableType.String: return InterviewDatabaseConstants.SqlTypes.String;
                default:
                    throw new ArgumentException("Unknown variable type: " + variable.Type);
            }
        }

        private void CreateSchema(DbConnection connection, TenantInfo tenant)
        {
            connection.Execute(commandBuilder.GenerateCreateSchema(tenant));
        }

        public async Task DropTenantSchemaAsync(string tenant)
        {
            List<string> tablesToDelete = new List<string>();

            using (var db = new NpgsqlConnection(connectionSettings.Value.DefaultConnection))
            {
                await db.OpenAsync();

                logger.LogInformation("Start drop tenant scheme: {tenant}", tenant);

                var schemas = (await db.QueryAsync<string>(
                    "select nspname from pg_catalog.pg_namespace " +
                    "where obj_description(nspname::regnamespace, 'pg_namespace') = @tenant",
                    new
                    {
                        tenant 
                    })).ToList();

                foreach (var schema in schemas)
                {
                    var tables = await db.QueryAsync<string>(
                        "select tablename from pg_tables where schemaname= @schema",
                        new { schema });

                    foreach (var table in tables)
                    {
                        tablesToDelete.Add($@"""{schema}"".""{table}""");
                    }
                }

                foreach (var tables in tablesToDelete.Batch(30))
                {
                    using (var tr = db.BeginTransaction())
                    {
                        foreach (var table in tables)
                        {
                            await db.ExecuteAsync($@"drop table if exists {table}");
                            logger.LogInformation("Dropped {table}", table);
                        }

                        await tr.CommitAsync();
                    }
                }

                using (var tr = db.BeginTransaction())
                {
                    foreach (var schema in schemas)
                    {
                        await db.ExecuteAsync($@"drop schema if exists ""{schema}""");
                        logger.LogInformation("Dropped schema {schema}.", schema);
                    }

                    await tr.CommitAsync();
                }
            }
        }


    }
}
