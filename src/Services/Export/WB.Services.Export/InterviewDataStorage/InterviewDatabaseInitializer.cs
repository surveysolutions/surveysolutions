using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Npgsql;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.InterviewDataStorage
{
    public interface IInterviewDatabaseInitializer
    {
        Task CreateQuestionnaireDbStructureAsync(TenantInfo tenant, QuestionnaireDocument questionnaireDocument);
    }


    public class InterviewDatabaseInitializer : IInterviewDatabaseInitializer
    {
        private readonly IOptions<DbConnectionSettings> connectionSettings;

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

        public InterviewDatabaseInitializer(IOptions<DbConnectionSettings> connectionSettings)
        {
            this.connectionSettings = connectionSettings;
        }

        public async Task CreateQuestionnaireDbStructureAsync(TenantInfo tenant, QuestionnaireDocument questionnaireDocument)
        {
            var connectionString = connectionSettings.Value.DefaultConnection;

            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var transaction = connection.BeginTransaction();

                await CreateSchemaAsync(connection, tenant);

                await CreateTablesRecursivelyAsync(connection, tenant, questionnaireDocument, rosterLevel: 0);

                await transaction.CommitAsync();
            }
        }

        private async Task CreateTablesRecursivelyAsync(NpgsqlConnection connection, TenantInfo tenant, Group group, int rosterLevel)
        {
            if (group.IsRoster)
                rosterLevel++;

            await CreateTableForGroupAsync(connection, tenant, group, rosterLevel);
            await CreateEnablementTableForGroupAsync(connection, tenant, group, rosterLevel);
            await CreateValidityTableForGroupAsync(connection, tenant, group, rosterLevel);

            var groups = group.Children.Where(entity => entity is Group).Cast<Group>();

            foreach (var nextGroup in groups)
            {
                await CreateTablesRecursivelyAsync(connection, tenant, nextGroup, rosterLevel);
            }
        }

        private Task CreateTableForGroupAsync(NpgsqlConnection connection, TenantInfo tenant, Group group, int rosterLevel)
        {
            var columns = new List<ColumnInfo>();
            columns.Add(new ColumnInfo(InterviewDatabaseConstants.InterviewId, "uuid", isPrimaryKey: true));
            if (rosterLevel > 0)
                columns.Add(new ColumnInfo(InterviewDatabaseConstants.RosterVector, $"float8[{rosterLevel}]", isPrimaryKey: true));

            var questions = group.Children.Where(entity => entity is Question).Cast<Question>().ToList();
            foreach (var question in questions)
                columns.Add(new ColumnInfo(question.ColumnName, GetSqlTypeForQuestion(question), isNullable: true));

            var variables = group.Children.Where(entity => entity is Variable).Cast<Variable>().ToList();
            foreach (var variable in variables)
                columns.Add(new ColumnInfo(variable.ColumnName, GetSqlTypeForVariable(variable), isNullable: true));

            if (!questions.Any() && !variables.Any())
                return Task.CompletedTask;

            var commandText = GenerateCreateTableScript(tenant.Name, group.TableName, columns);
            return ExecuteCommandAsync(connection, commandText);
        }

        private Task CreateEnablementTableForGroupAsync(NpgsqlConnection connection, TenantInfo tenant, Group group, int rosterLevel)
        {
            var columns = new List<ColumnInfo>();
            columns.Add(new ColumnInfo(InterviewDatabaseConstants.InterviewId, "uuid", isPrimaryKey: true));
            if (rosterLevel > 0)
                columns.Add(new ColumnInfo(InterviewDatabaseConstants.RosterVector, $"float8[{rosterLevel}]", isPrimaryKey: true));

            var questions = group.Children.Where(entity => entity is Question).Cast<Question>().ToList();
            foreach (var question in questions)
                columns.Add(new ColumnInfo(question.ColumnName, "bool", isNullable: false, defaultValue: "true"));

            var variables = group.Children.Where(entity => entity is Variable).Cast<Variable>().ToList();
            foreach (var variable in variables)
                columns.Add(new ColumnInfo(variable.ColumnName, "bool", isNullable: false, defaultValue: "true"));

            if (!questions.Any() && !variables.Any())
                return Task.CompletedTask;

            var commandText = GenerateCreateTableScript(tenant.Name, group.EnablementTableName, columns);
            return ExecuteCommandAsync(connection, commandText);
        }

        private Task CreateValidityTableForGroupAsync(NpgsqlConnection connection, TenantInfo tenant, Group group, int rosterLevel)
        {
            var columns = new List<ColumnInfo>();
            columns.Add(new ColumnInfo(InterviewDatabaseConstants.InterviewId, "uuid", isPrimaryKey: true));
            if (rosterLevel > 0)
                columns.Add(new ColumnInfo(InterviewDatabaseConstants.RosterVector, $"float8[{rosterLevel}]", isPrimaryKey: true));

            var questions = group.Children.Where(entity => entity is Question).Cast<Question>().ToList();
            foreach (var question in questions)
                columns.Add(new ColumnInfo(question.ColumnName, "int4[]", isNullable: true));

            if (!questions.Any())
                return Task.CompletedTask;

            var commandText = GenerateCreateTableScript(tenant.Name, group.ValidityTableName, columns);
            return ExecuteCommandAsync(connection, commandText);
        }

        private string GetSqlTypeForQuestion(Question question)
        {
            switch (question)
            {
                case TextQuestion textQuestion : return "text";
                case NumericQuestion numericQuestion when (numericQuestion.IsInteger): return "int4";
                case NumericQuestion numericQuestion when (!numericQuestion.IsInteger): return "float8";
                case TextListQuestion textListQuestion : return "text[]";
                case MultimediaQuestion multimediaQuestion : return "text";
                case DateTimeQuestion dateTimeQuestion : return "timestamp";
                case AudioQuestion audioQuestion : return "jsonb";
                case AreaQuestion areaQuestion : return "jsonb";
                case GpsCoordinateQuestion gpsCoordinateQuestion : return "jsonb";
                case QRBarcodeQuestion qrBarcodeQuestion : return "text";
                case SingleQuestion singleQuestion 
                    when (singleQuestion.LinkedToQuestionId.HasValue || singleQuestion.LinkedToRosterId.HasValue):
                    return "float8[]";
                case SingleQuestion singleQuestion 
                    when (!singleQuestion.LinkedToQuestionId.HasValue && !singleQuestion.LinkedToRosterId.HasValue):
                    return "float8";
                case MultyOptionsQuestion multiOptionsQuestion
                    when(multiOptionsQuestion.LinkedToQuestionId.HasValue || multiOptionsQuestion.LinkedToRosterId.HasValue):
                    return "float8[][]";
                case MultyOptionsQuestion multiOptionsQuestion
                    when(!multiOptionsQuestion.LinkedToQuestionId.HasValue && !multiOptionsQuestion.LinkedToRosterId.HasValue):
                    return "float8[]";
                default:
                    throw new ArgumentException("Unknown question type: " + question.GetType().Name);
            }
        }

        private string GetSqlTypeForVariable(Variable variable)
        {
            switch (variable.Type)
            {
                case VariableType.Boolean: return "bool";
                case VariableType.DateTime: return "timestamp";
                case VariableType.Double: return "float8";
                case VariableType.LongInteger: return "int8";
                case VariableType.String: return "text";
                default: 
                    throw new ArgumentException("Unknown variable type: " + variable.Type);
            }
        }

        private static Task CreateSchemaAsync(NpgsqlConnection connection, TenantInfo tenant)
        {
            return ExecuteCommandAsync(connection, $"CREATE SCHEMA IF NOT EXISTS \"{tenant.Name}\"");
        }

        private string GenerateCreateTableScript(string schemaName, string tableName, List<ColumnInfo> columns)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE IF NOT EXISTS \"{schemaName}\".\"{tableName}\"(");

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

        private static Task ExecuteCommandAsync(NpgsqlConnection connection, string commandText)
        {
            var createTableCommand = connection.CreateCommand();
            createTableCommand.CommandText = commandText;
            return createTableCommand.ExecuteNonQueryAsync();
        }
    }
}
