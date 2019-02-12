using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.InterviewDataStorage
{
    public interface IInterviewDatabaseInitializer
    {
        void CreateQuestionnaireDbStructure(ITenantContext tenantContext, QuestionnaireDocument questionnaireDocument);
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

        public InterviewDatabaseInitializer(IOptions<DbConnectionSettings> connectionSettings, ITenantContext tenantContext)
        {
            this.connectionSettings = connectionSettings;
        }

        public void CreateQuestionnaireDbStructure(ITenantContext tenantContext, QuestionnaireDocument questionnaireDocument)
        {
            using(var dbContext = new TenantDbContext(tenantContext, connectionSettings))
            {
                dbContext.Database.BeginTransaction();
                var connection = dbContext.Database.GetDbConnection();
                
                CreateSchema(connection, tenantContext.Tenant);

                foreach (var storedGroup in questionnaireDocument.GetAllStoredGroups())
                {
                    CreateTableForGroup(connection, storedGroup, questionnaireDocument);
                    CreateEnablementTableForGroup(connection, storedGroup);
                    CreateValidityTableForGroup(connection, storedGroup);
                }

                dbContext.Database.CommitTransaction();
                dbContext.SaveChanges();
            }
        }

        private void CreateTableForGroup(DbConnection connection, Group group, QuestionnaireDocument questionnaireDocument)
        {
            var questions = group.Children.Where(entity => entity is Question).Cast<Question>().ToList();
            var variables = group.Children.Where(entity => entity is Variable).Cast<Variable>().ToList();
            if (!group.IsRoster && !questions.Any() && !variables.Any())
                return;

            var columns = new List<ColumnInfo>();
            columns.Add(new ColumnInfo(InterviewDatabaseConstants.InterviewId, "uuid", isPrimaryKey: true));
            if (group.RosterLevel > 0)
                columns.Add(new ColumnInfo(InterviewDatabaseConstants.RosterVector, $"int4[{group.RosterLevel}]", isPrimaryKey: true));

            foreach (var question in questions)
                columns.Add(new ColumnInfo(question.ColumnName, GetSqlTypeForQuestion(question, questionnaireDocument), isNullable: true));

            foreach (var variable in variables)
                columns.Add(new ColumnInfo(variable.ColumnName, GetSqlTypeForVariable(variable), isNullable: true));

            var commandText = GenerateCreateTableScript(group.TableName, columns);
            connection.Execute(commandText);
        }

        private void CreateEnablementTableForGroup(DbConnection connection, Group group)
        {
            var questions = group.Children.Where(entity => entity is Question).Cast<Question>().ToList();
            var variables = group.Children.Where(entity => entity is Variable).Cast<Variable>().ToList();
            if (!group.IsRoster && !questions.Any() && !variables.Any())
                return;

            var columns = new List<ColumnInfo>();
            columns.Add(new ColumnInfo(InterviewDatabaseConstants.InterviewId, "uuid", isPrimaryKey: true));
            if (group.RosterLevel > 0)
                columns.Add(new ColumnInfo(InterviewDatabaseConstants.RosterVector, $"int4[{group.RosterLevel}]", isPrimaryKey: true));

            columns.Add(new ColumnInfo(InterviewDatabaseConstants.InstanceValue, "bool", defaultValue: "true"));

            foreach (var question in questions)
                columns.Add(new ColumnInfo(question.ColumnName, "bool", isNullable: false, defaultValue: "true"));

            foreach (var variable in variables)
                columns.Add(new ColumnInfo(variable.ColumnName, "bool", isNullable: false, defaultValue: "true"));

            var commandText = GenerateCreateTableScript(group.EnablementTableName, columns);
            connection.Execute(commandText);
        }

        private void CreateValidityTableForGroup(DbConnection connection, Group group)
        {
            var questions = group.Children.Where(entity => entity is Question).Cast<Question>().ToList();
            if (!questions.Any())
                return;

            var columns = new List<ColumnInfo>();
            columns.Add(new ColumnInfo(InterviewDatabaseConstants.InterviewId, "uuid", isPrimaryKey: true));
            if (group.RosterLevel > 0)
                columns.Add(new ColumnInfo(InterviewDatabaseConstants.RosterVector, $"int4[{group.RosterLevel}]", isPrimaryKey: true));

            foreach (var question in questions)
                columns.Add(new ColumnInfo(question.ColumnName, "int4[]", isNullable: true));

            var commandText = GenerateCreateTableScript(group.ValidityTableName, columns);
            connection.Execute(commandText);
        }

        private string GetSqlTypeForQuestion(Question question, QuestionnaireDocument questionnaire)
        {
            switch (question)
            {
                case TextQuestion _ : return "text";
                case NumericQuestion numericQuestion when (numericQuestion.IsInteger): return "int4";
                case NumericQuestion numericQuestion when (!numericQuestion.IsInteger): return "float8";
                case TextListQuestion _ : return "jsonb";
                case MultimediaQuestion _ : return "text";
                case DateTimeQuestion dateTimeQuestion : return "timestamp";
                case AudioQuestion audioQuestion : return "jsonb";
                case AreaQuestion areaQuestion : return "jsonb";
                case GpsCoordinateQuestion gpsCoordinateQuestion : return "jsonb";
                case QRBarcodeQuestion qrBarcodeQuestion : return "text";
                case SingleQuestion singleQuestion when (singleQuestion.LinkedToRosterId.HasValue):
                    return "int4[]";
                case SingleQuestion singleQuestion when (singleQuestion.LinkedToQuestionId.HasValue):
                    var singleSourceQuestion = questionnaire.Find<Question>(singleQuestion.LinkedToQuestionId.Value);
                    return singleSourceQuestion is TextListQuestion ? "int4" : "int4[]";
                case SingleQuestion singleQuestion 
                    when (!singleQuestion.LinkedToQuestionId.HasValue && !singleQuestion.LinkedToRosterId.HasValue):
                    return "int4";
                case MultyOptionsQuestion yesNoOptionsQuestion when (yesNoOptionsQuestion.YesNoView):
                case MultyOptionsQuestion multiLinkedToRoster when (multiLinkedToRoster.LinkedToRosterId.HasValue):
                    return "jsonb";
                case MultyOptionsQuestion multiLinkedToQuestion when (multiLinkedToQuestion.LinkedToQuestionId.HasValue):
                    var multiSourceQuestion = questionnaire.Find<Question>(multiLinkedToQuestion.LinkedToQuestionId.Value);
                    return multiSourceQuestion is TextListQuestion ? "int4[]" : "jsonb";
                case MultyOptionsQuestion multiOptionsQuestion
                    when(!multiOptionsQuestion.LinkedToQuestionId.HasValue && !multiOptionsQuestion.LinkedToRosterId.HasValue):
                    return "int4[]";
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

        private static void CreateSchema(DbConnection connection, TenantInfo tenant)
        {
            connection.Execute($"CREATE SCHEMA IF NOT EXISTS \"{tenant.SchemaName()}\"");
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
