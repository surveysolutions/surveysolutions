using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.InterviewDataStorage
{
    public interface IInterviewDatabaseInitializer
    {
        void CreateQuestionnaireDbStructure(TenantInfo tenant, QuestionnaireDocument questionnaireDocument);
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

        public void CreateQuestionnaireDbStructure(TenantInfo tenant, QuestionnaireDocument questionnaireDocument)
        {
            var connectionString = connectionSettings.Value.DefaultConnection;

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                CreateSchema(connection, tenant);

                foreach (var storedGroup in questionnaireDocument.GetAllStoredGroups())
                {
                    CreateTableForGroup(connection, tenant, storedGroup);
                    CreateEnablementTableForGroup(connection, tenant, storedGroup);
                    CreateValidityTableForGroup(connection, tenant, storedGroup);
                }

                transaction.Commit();
            }
        }

        private void CreateTableForGroup(NpgsqlConnection connection, TenantInfo tenant, Group group)
        {
            var columns = new List<ColumnInfo>();
            columns.Add(new ColumnInfo(InterviewDatabaseConstants.InterviewId, "uuid", isPrimaryKey: true));
            if (group.RosterLevel > 0)
                columns.Add(new ColumnInfo(InterviewDatabaseConstants.RosterVector, $"int8[{group.RosterLevel}]", isPrimaryKey: true));

            var questions = group.Children.Where(entity => entity is Question).Cast<Question>().ToList();
            foreach (var question in questions)
                columns.Add(new ColumnInfo(question.ColumnName, GetSqlTypeForQuestion(question), isNullable: true));

            var variables = group.Children.Where(entity => entity is Variable).Cast<Variable>().ToList();
            foreach (var variable in variables)
                columns.Add(new ColumnInfo(variable.ColumnName, GetSqlTypeForVariable(variable), isNullable: true));

            if (!questions.Any() && !variables.Any())
                return ;

            var commandText = GenerateCreateTableScript(tenant.Name, group.TableName, columns);
            connection.Execute(commandText);
        }

        private void CreateEnablementTableForGroup(NpgsqlConnection connection, TenantInfo tenant, Group group)
        {
            var columns = new List<ColumnInfo>();
            columns.Add(new ColumnInfo(InterviewDatabaseConstants.InterviewId, "uuid", isPrimaryKey: true));
            if (group.RosterLevel > 0)
                columns.Add(new ColumnInfo(InterviewDatabaseConstants.RosterVector, $"int8[{group.RosterLevel}]", isPrimaryKey: true));

            //columns.Add(new ColumnInfo(InterviewDatabaseConstants.InstanceValue, "bool", defaultValue: "true"));

            var questions = group.Children.Where(entity => entity is Question).Cast<Question>().ToList();
            foreach (var question in questions)
                columns.Add(new ColumnInfo(question.ColumnName, "bool", isNullable: false, defaultValue: "true"));

            var variables = group.Children.Where(entity => entity is Variable).Cast<Variable>().ToList();
            foreach (var variable in variables)
                columns.Add(new ColumnInfo(variable.ColumnName, "bool", isNullable: false, defaultValue: "true"));

            if (!questions.Any() && !variables.Any())
                return;
            var commandText = GenerateCreateTableScript(tenant.Name, group.EnablementTableName, columns);
            connection.Execute(commandText);
        }

        private void CreateValidityTableForGroup(NpgsqlConnection connection, TenantInfo tenant, Group group)
        {
            var columns = new List<ColumnInfo>();
            columns.Add(new ColumnInfo(InterviewDatabaseConstants.InterviewId, "uuid", isPrimaryKey: true));
            if (group.RosterLevel > 0)
                columns.Add(new ColumnInfo(InterviewDatabaseConstants.RosterVector, $"int8[{group.RosterLevel}]", isPrimaryKey: true));

            //columns.Add(new ColumnInfo(InterviewDatabaseConstants.InstanceValue, "int4[]", isNullable: true));

            var questions = group.Children.Where(entity => entity is Question).Cast<Question>().ToList();
            foreach (var question in questions)
                columns.Add(new ColumnInfo(question.ColumnName, "int4[]", isNullable: true));

            if (!questions.Any())
                return;

            var commandText = GenerateCreateTableScript(tenant.Name, group.ValidityTableName, columns);
            connection.Execute(commandText);
        }

        private string GetSqlTypeForQuestion(Question question)
        {
            switch (question)
            {
                case TextQuestion _ : return "text";
                case NumericQuestion numericQuestion when (numericQuestion.IsInteger): return "int4";
                case NumericQuestion numericQuestion when (!numericQuestion.IsInteger): return "float8";
                case TextListQuestion _ : return "text[]";
                case MultimediaQuestion _ : return "text";
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

        private static void CreateSchema(NpgsqlConnection connection, TenantInfo tenant)
        {
            connection.Execute($"CREATE SCHEMA IF NOT EXISTS \"{tenant.Name}\"");
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
    }
}
