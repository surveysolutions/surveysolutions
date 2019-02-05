using System;
using System.Collections.Generic;
using System.Linq;
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
            public ColumnInfo(string name, string sqlType, bool isPrimaryKey = false, bool isUnique = false)
            {
                Name = name;
                SqlType = sqlType;
                IsPrimaryKey = isPrimaryKey;
                IsUnique = isUnique;
                Nullable = isPrimaryKey || isUnique ? "NOT NULL" : "NULL";
            }

            public string Name { get; }
            public string SqlType { get; }
            public bool IsPrimaryKey { get; }
            public bool IsUnique { get; }
            public string Nullable { get; }
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
            var tableName = $"\"{tenant.Name}\".\"{group.TableName}\"";

            var createTableCommand = connection.CreateCommand();

            var columns = GetColumnsInfoFor(group, rosterLevel);

            var commandText = $"CREATE TABLE IF NOT EXISTS {tableName}(" +
                              string.Join(" , ", columns.Select(c => $"\"{c.Name}\" {c.SqlType} {c.Nullable}")) + $" , " +
                              $"PRIMARY KEY ({string.Join(" , ", columns.Where(c => c.IsPrimaryKey).Select(c => c.Name))})" +
                              $")";

            createTableCommand.CommandText = commandText;
            return createTableCommand.ExecuteNonQueryAsync();
        }

        private Task CreateEnablementTableForGroupAsync(NpgsqlConnection connection, TenantInfo tenant, Group group, int rosterLevel)
        {
            var tableName = $"\"{tenant.Name}\".\"{group.EnablementTableName}\"";

            var createTableCommand = connection.CreateCommand();

            var columns = GetColumnsInfoFor(group, rosterLevel);

            var commandText = $"CREATE TABLE IF NOT EXISTS {tableName}(" +
                              string.Join(" , ", columns.Select(c => $"\"{c.Name}\" bool NOT NULL DEFAULT true")) + $" , " +
                              $"PRIMARY KEY ({string.Join(" , ", columns.Where(c => c.IsPrimaryKey).Select(c => c.Name))})" +
                              $")";

            createTableCommand.CommandText = commandText;
            return createTableCommand.ExecuteNonQueryAsync();
        }

        private Task CreateValidityTableForGroupAsync(NpgsqlConnection connection, TenantInfo tenant, Group group, int rosterLevel)
        {
            var tableName = $"\"{tenant.Name}\".\"{group.ValidityTableName}\"";

            var createTableCommand = connection.CreateCommand();

            var columns = GetColumnsInfoFor(group, rosterLevel);

            var commandText = $"CREATE TABLE IF NOT EXISTS {tableName}(" +
                              string.Join(" , ", columns.Select(c => $"\"{c.Name}\" int4[] {c.Nullable}")) + $" , " +
                              $"PRIMARY KEY ({string.Join(" , ", columns.Where(c => c.IsPrimaryKey).Select(c => c.Name))})" +
                              $")";

            createTableCommand.CommandText = commandText;
            return createTableCommand.ExecuteNonQueryAsync();
        }

        private List<ColumnInfo> GetColumnsInfoFor(Group @group, int rosterLevel)
        {
            var columns = new List<ColumnInfo>();

            columns.Add(new ColumnInfo("interview_id", "uuid", isPrimaryKey: true));

            //if (group is QuestionnaireDocument)
            //    columns.Add(new ColumnInfo("interview_key", "varchar", isUnique: true));

            if (rosterLevel > 0)
                columns.Add(new ColumnInfo("roster_vector", $"float8[{rosterLevel}]", isPrimaryKey: true));

            var questions = group.Children.Where(entity => entity is Question).Cast<Question>();
            foreach (var question in questions)
                columns.Add(new ColumnInfo(question.ColumnName, GetSqlTypeForQuestion(question)));

            var variables = group.Children.Where(entity => entity is Variable).Cast<Variable>();
            foreach (var variable in variables)
                columns.Add(new ColumnInfo(variable.ColumnName, GetSqlTypeForVariable(variable)));

            return columns;
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
            var checkSchemaExistsCommand = connection.CreateCommand();
            checkSchemaExistsCommand.CommandText = $"CREATE SCHEMA IF NOT EXISTS \"{tenant.Name}\"";
            return checkSchemaExistsCommand.ExecuteNonQueryAsync();
        }
    }
}
