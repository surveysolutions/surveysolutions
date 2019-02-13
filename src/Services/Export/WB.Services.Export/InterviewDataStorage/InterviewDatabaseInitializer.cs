using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Dapper;
using Microsoft.EntityFrameworkCore;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.InterviewDataStorage
{
    public interface IInterviewDatabaseInitializer
    {
        void CreateQuestionnaireDbStructure(QuestionnaireDocument questionnaireDocument);
    }

    public class InterviewDatabaseInitializer : IInterviewDatabaseInitializer
    {
        private readonly ITenantContext tenantContext;

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
        public InterviewDatabaseInitializer(ITenantContext tenantContext)
        {
            this.tenantContext = tenantContext;
        }

        public void CreateQuestionnaireDbStructure(QuestionnaireDocument questionnaireDocument)
        {
            var dbContext = tenantContext.DbContext;
            var key = tenantContext.Tenant.SchemaName() + questionnaireDocument.QuestionnaireId.Id;
            if (createdQuestionnaireTables.Contains(key))
                return;

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                var connection = dbContext.Database.GetDbConnection();

                CreateSchema(connection, tenantContext.Tenant);

                foreach (var storedGroup in questionnaireDocument.GetAllStoredGroups())
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

        private void CreateTableForGroup(DbConnection connection, Group group, QuestionnaireDocument questionnaireDocument)
        {
            if (!group.DoesSupportDataTable)
                return;

            var columns = new List<ColumnInfo>();
            columns.Add(new ColumnInfo(InterviewDatabaseConstants.InterviewId, "uuid", isPrimaryKey: true));
            if (group.RosterLevel > 0)
                columns.Add(new ColumnInfo(InterviewDatabaseConstants.RosterVector, $"int4[{group.RosterLevel}]", isPrimaryKey: true));

            var questions = group.Children.Where(entity => entity is Question).Cast<Question>();
            foreach (var question in questions)
                columns.Add(new ColumnInfo(question.ColumnName, GetSqlTypeForQuestion(question, questionnaireDocument), isNullable: true));

            var variables = group.Children.Where(entity => entity is Variable).Cast<Variable>();
            foreach (var variable in variables)
                columns.Add(new ColumnInfo(variable.ColumnName, GetSqlTypeForVariable(variable), isNullable: true));

            var commandText = GenerateCreateTableScript(group.TableName, columns);
            connection.Execute(commandText);
        }

        private void CreateEnablementTableForGroup(DbConnection connection, Group group)
        {
            if (!group.DoesSupportEnablementTable)
                return;

            var columns = new List<ColumnInfo>();
            columns.Add(new ColumnInfo(InterviewDatabaseConstants.InterviewId, "uuid", isPrimaryKey: true));
            if (group.RosterLevel > 0)
                columns.Add(new ColumnInfo(InterviewDatabaseConstants.RosterVector, $"int4[{group.RosterLevel}]", isPrimaryKey: true));

            columns.Add(new ColumnInfo(InterviewDatabaseConstants.InstanceValue, "bool", defaultValue: "true"));

            var questions = group.Children.Where(entity => entity is Question).Cast<Question>();
            foreach (var question in questions)
                columns.Add(new ColumnInfo(question.ColumnName, "bool", isNullable: false, defaultValue: "true"));

            var variables = group.Children.Where(entity => entity is Variable).Cast<Variable>();
            foreach (var variable in variables)
                columns.Add(new ColumnInfo(variable.ColumnName, "bool", isNullable: false, defaultValue: "true"));

            var commandText = GenerateCreateTableScript(group.EnablementTableName, columns);
            connection.Execute(commandText);
        }

        private void CreateValidityTableForGroup(DbConnection connection, Group group)
        {
            if (!group.DoesSupportValidityTable)
                return;

            var columns = new List<ColumnInfo>();
            columns.Add(new ColumnInfo(InterviewDatabaseConstants.InterviewId, "uuid", isPrimaryKey: true));
            if (group.RosterLevel > 0)
                columns.Add(new ColumnInfo(InterviewDatabaseConstants.RosterVector, $"int4[{group.RosterLevel}]", isPrimaryKey: true));

            var questions = group.Children.Where(entity => entity is Question).Cast<Question>();
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
