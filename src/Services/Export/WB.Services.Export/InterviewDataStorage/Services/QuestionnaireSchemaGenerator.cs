using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.InterviewDataStorage.InterviewDataExport;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.Storage;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.InterviewDataStorage.Services
{
    public class QuestionnaireSchemaGenerator : IQuestionnaireSchemaGenerator
    {
        private readonly ITenantContext tenantContext;
        private readonly TenantDbContext dbContext;
        private readonly IDatabaseSchemaCommandBuilder commandBuilder;
        private readonly ILogger<DatabaseSchemaService> logger;

        public QuestionnaireSchemaGenerator(
            ITenantContext tenantContext,
            TenantDbContext dbContext,
            IDatabaseSchemaCommandBuilder commandBuilder,
            ILogger<DatabaseSchemaService> logger)
        {
            this.tenantContext = tenantContext;
            this.dbContext = dbContext;
            this.commandBuilder = commandBuilder;
            this.logger = logger;
        }

        public void CreateQuestionnaireDbStructure(QuestionnaireDocument questionnaireDocument)
        {
            try
            {
                // required to prevent same schema generation in parallel
                // can and already happen if HQ query info on export status
                // while export job start running
                // Lock will be released on transaction commit at the end of current export processing batch
                this.dbContext.AcquireXactLock(TenantDbContext.SchemaChangesLock);
                
                var connection = dbContext.Database.GetDbConnection();

                CreateSchema(connection, tenantContext.Tenant);

                foreach (var storedGroup in questionnaireDocument.DatabaseStructure.GetAllLevelTables())
                {
                    CreateTableForGroup(connection, storedGroup, questionnaireDocument);
                    CreateEnablementTableForGroup(connection, storedGroup);
                    CreateValidityTableForGroup(connection, storedGroup);
                }

                logger.LogInformation("Created database structure for {tenantName} ({questionnaireId} [{table}])",
                    this.tenantContext.Tenant?.Name, questionnaireDocument.QuestionnaireId,
                    questionnaireDocument.TableName);
            }
            catch (Exception e)
            {
                e.Data.Add("WB:questionnaireId", questionnaireDocument.QuestionnaireId.ToString());
                e.Data.Add("WB:questionnaireTitle", questionnaireDocument.Title);
                throw;
            }
        }

        public void DropQuestionnaireDbStructure(QuestionnaireDocument questionnaireDocument)
        {
            try
            {
                this.dbContext.AcquireXactLock(TenantDbContext.SchemaChangesLock);
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
                e.Data.Add("WB:questionnaireId", questionnaireDocument.QuestionnaireId.ToString());
                e.Data.Add("WB:questionnaireTitle", questionnaireDocument.Title);
                throw;
            }
        }

        private void CreateTableForGroup(DbConnection connection, QuestionnaireLevelDatabaseTable group, QuestionnaireDocument questionnaireDocument)
        {
            var columns = new List<CreateTableColumnInfo>();
            columns.Add(new CreateTableColumnInfo(InterviewDatabaseConstants.InterviewId, InterviewDatabaseConstants.SqlTypes.Guid, isPrimaryKey: true));
            if (group.IsRoster)
                columns.Add(new CreateTableColumnInfo(InterviewDatabaseConstants.RosterVector, InterviewDatabaseConstants.SqlTypes.IntArray, isPrimaryKey: true));

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

        public Task DropTenantSchemaAsync(string tenant, CancellationToken cancellationToken = default)
        {
            return dbContext.DropTenantSchemaAsync(tenant, cancellationToken);
        }
    }
}
