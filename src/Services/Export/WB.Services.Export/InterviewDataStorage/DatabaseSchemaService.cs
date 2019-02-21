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

namespace WB.Services.Export.InterviewDataStorage
{
    public class DatabaseSchemaService : IDatabaseSchemaService
    {
        private readonly ITenantContext tenantContext;
        private readonly TenantDbContext dbContext;

        private class ColumnInfo
        {
            public ColumnInfo(string name, string sqlType, bool isPrimaryKey = false, bool isNullable = false, string defaultValue = null)
            {
                Name = name;
                SqlType = sqlType;
                IsPrimaryKey = isPrimaryKey;
                DefaultValue = defaultValue;
                IsNullable = isNullable;
            }

            public string Name { get; }
            public string SqlType { get; }
            public bool IsPrimaryKey { get; }
            public string DefaultValue { get; }
            public bool IsNullable { get; }
        }

        private static readonly HashSet<string> createdQuestionnaireTables = new HashSet<string>();
        public DatabaseSchemaService(ITenantContext tenantContext, TenantDbContext dbContext)
        {
            this.tenantContext = tenantContext;
            this.dbContext = dbContext;
        }

        public void CreateQuestionnaireDbStructure(QuestionnaireDocument questionnaireDocument)
        {
            var key = tenantContext.Tenant.SchemaName() + questionnaireDocument.QuestionnaireId.Id;
            if (createdQuestionnaireTables.Contains(key))
                return;

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                var connection = dbContext.Database.GetDbConnection();

                CreateSchema(connection, tenantContext.Tenant);

                foreach (var storedGroup in questionnaireDocument.DatabaseStructure.GetAllLevelTables())
                {
                    CreateTableForGroup(connection, storedGroup, questionnaireDocument);
                    CreateEnablementTableForGroup(connection, storedGroup);
                    CreateValidityTableForGroup(connection, storedGroup);
                }

                transaction.Commit();
                dbContext.SaveChanges();
            }

            createdQuestionnaireTables.Add(key);
        }

        public void DropQuestionnaireDbStructure(QuestionnaireDocument questionnaireDocument)
        {
            var db = dbContext.Database.GetDbConnection();
            foreach (var storedGroup in questionnaireDocument.DatabaseStructure.GetAllLevelTables())
            {
                db.Execute($"DROP TABLE IF EXISTS \"{storedGroup.TableName}\" CASCADE ");
                db.Execute($"DROP TABLE IF EXISTS \"{storedGroup.EnablementTableName}\" CASCADE");
                db.Execute($"DROP TABLE IF EXISTS \"{storedGroup.ValidityTableName}\" CASCADE");
            }
        }

        private void CreateTableForGroup(DbConnection connection, QuestionnaireLevelDatabaseTable group, QuestionnaireDocument questionnaireDocument)
        {
            var columns = new List<ColumnInfo>();
            columns.Add(new ColumnInfo(InterviewDatabaseConstants.InterviewId, InterviewDatabaseConstants.SqlTypes.Guid, isPrimaryKey: true));
            if (group.IsRoster)
                columns.Add(new ColumnInfo(InterviewDatabaseConstants.RosterVector, $"int4[]", isPrimaryKey: true));

            var questions = group.DataColumns.Where(entity => entity is Question).Cast<Question>();
            foreach (var question in questions)
                columns.Add(new ColumnInfo(question.ColumnName, GetSqlTypeForQuestion(question, questionnaireDocument), isNullable: true));

            var variables = group.DataColumns.Where(entity => entity is Variable).Cast<Variable>();
            foreach (var variable in variables)
                columns.Add(new ColumnInfo(variable.ColumnName, GetSqlTypeForVariable(variable), isNullable: true));

            var commandText = GenerateCreateTableScript(group.TableName, columns);
            connection.Execute(commandText);
        }

        private void CreateEnablementTableForGroup(DbConnection connection, QuestionnaireLevelDatabaseTable group)
        {
            var columns = new List<ColumnInfo>();
            columns.Add(new ColumnInfo(InterviewDatabaseConstants.InterviewId, InterviewDatabaseConstants.SqlTypes.Guid, isPrimaryKey: true));
            if (group.IsRoster)
                columns.Add(new ColumnInfo(InterviewDatabaseConstants.RosterVector, $"int4[]", isPrimaryKey: true));

            var questions = group.EnablementColumns.Where(entity => entity is Question).Cast<Question>();
            foreach (var question in questions)
                columns.Add(new ColumnInfo(question.ColumnName, InterviewDatabaseConstants.SqlTypes.Bool, isNullable: false, defaultValue: "true"));

            var variables = group.EnablementColumns.Where(entity => entity is Variable).Cast<Variable>();
            foreach (var variable in variables)
                columns.Add(new ColumnInfo(variable.ColumnName, InterviewDatabaseConstants.SqlTypes.Bool, isNullable: false, defaultValue: "true"));

            var staticTexts = group.EnablementColumns.Where(entity => entity is StaticText).Cast<StaticText>();
            foreach (var staticText in staticTexts)
                columns.Add(new ColumnInfo(staticText.ColumnName, InterviewDatabaseConstants.SqlTypes.Bool, isNullable: false, defaultValue: "true"));

            var nonRosters = group.EnablementColumns.Where(entity => entity is Group).Cast<Group>();
            foreach (var notRoster in nonRosters)
                columns.Add(new ColumnInfo(notRoster.ColumnName, InterviewDatabaseConstants.SqlTypes.Bool, isNullable: false, defaultValue: "true"));

            var commandText = GenerateCreateTableScript(group.EnablementTableName, columns);
            connection.Execute(commandText);
        }

        private void CreateValidityTableForGroup(DbConnection connection, QuestionnaireLevelDatabaseTable group)
        {
            var columns = new List<ColumnInfo>();
            columns.Add(new ColumnInfo(InterviewDatabaseConstants.InterviewId, InterviewDatabaseConstants.SqlTypes.Guid, isPrimaryKey: true));
            if (group.IsRoster)
                columns.Add(new ColumnInfo(InterviewDatabaseConstants.RosterVector, $"int4[]", isPrimaryKey: true));

            var questions = group.ValidityColumns.Where(entity => entity is Question).Cast<Question>();
            foreach (var question in questions)
                columns.Add(new ColumnInfo(question.ColumnName, InterviewDatabaseConstants.SqlTypes.IntArray, isNullable: true));

            var staticTexts = group.ValidityColumns.Where(entity => entity is StaticText).Cast<StaticText>();
            foreach (var staticText in staticTexts)
                columns.Add(new ColumnInfo(staticText.ColumnName, InterviewDatabaseConstants.SqlTypes.IntArray, isNullable: true));

            var commandText = GenerateCreateTableScript(group.ValidityTableName, columns);
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

        private static void CreateSchema(DbConnection connection, TenantInfo tenant)
        {
            connection.Execute($"CREATE SCHEMA IF NOT EXISTS \"{tenant.SchemaName()}\"");
        }

        public static async Task DropTenantSchemaAsync(IOptions<DbConnectionSettings> connectionSettings, string tenant,
            ILogger<DatabaseSchemaService> logger)
        {
            List<string> tablesToDelete = new List<string>();

            using (var db = new NpgsqlConnection(connectionSettings.Value.DefaultConnection))
            {
                await db.OpenAsync();

                logger.LogInformation("Start drop tenant scheme: {tenant}", tenant);

                var schemas = await db.QueryAsync<string>(
                    "select nspname from pg_catalog.pg_namespace where nspname like @tenant " +
                    "and exists (select tablename from pg_tables where schemaname= nspname and tablename = '__migrations')",
                    new
                    {
                        tenant = tenant + "_%"
                    });

                foreach (var schema in schemas)
                {
                    var tables = await db.QueryAsync<string>("select tablename from pg_tables where schemaname= @schema",
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

        private string GenerateCreateTableScript(string tableName, List<ColumnInfo> columns)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE IF NOT EXISTS \"{tableName}\"(");

            foreach (var column in columns)
            {
                sb.Append($"\"{column.Name}\" {column.SqlType} ");
                sb.Append(column.IsNullable ? " NULL " : " NOT NULL ");
                if (column.DefaultValue != null)
                    sb.Append(" DEFAULT " + column.DefaultValue);
                sb.AppendLine(",");
            }

            sb.AppendLine($"PRIMARY KEY ({string.Join(" , ", columns.Where(c => c.IsPrimaryKey).Select(c => $"\"{c.Name}\""))})");
            sb.AppendLine(")");
            return sb.ToString();
        }
    }
}
