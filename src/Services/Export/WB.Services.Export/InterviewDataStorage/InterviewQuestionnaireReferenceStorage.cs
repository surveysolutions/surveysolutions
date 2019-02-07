using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Storage;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.InterviewDataStorage
{
    public interface IInterviewQuestionnaireReferenceStorage
    {
        Task<QuestionnaireId> GetQuestionnaireIdByInterviewIdAsync(Guid interviewId, CancellationToken cancellationToken);

        Task AddInterviewQuestionnaireReferenceAsync(Guid interviewId, QuestionnaireId questionnaireId, CancellationToken cancellationToken);

        Task RemoveInterviewQuestionnaireReferenceAsync(Guid interviewId, CancellationToken cancellationToken);
    }

    public class InterviewQuestionnaireReferenceStorage : IInterviewQuestionnaireReferenceStorage
    {
        private readonly ITenantContext tenantContext;
        private const string InterviewQuestionnaireReferenceTableName = "interview_questionnaire_reference";
        private const string InterviewColumnName = "interviewid";
        private const string QuestionnaireColumnName = "questionnaireid";

        public InterviewQuestionnaireReferenceStorage(ITenantContext tenantContext)
        {
            this.tenantContext = tenantContext;
            EnshureTableExists();
        }

        public async Task<QuestionnaireId> GetQuestionnaireIdByInterviewIdAsync(Guid interviewId, CancellationToken cancellationToken)
        {
            var commandText = $"SELECT questionnaireid FROM \"{tenantContext.Tenant.Name}\".\"{InterviewQuestionnaireReferenceTableName}\"" +
                              $"  WHERE {InterviewColumnName} = @interviewId;";
            var command = this.tenantContext.Connection.CreateCommand();
            command.CommandText = commandText;
            AddParameter(command, "@interviewId", DbType.Guid, interviewId);
            var questionnaireId = (await command.ExecuteScalarAsync(cancellationToken)) as string;
            return new QuestionnaireId(questionnaireId);
        }

        public Task AddInterviewQuestionnaireReferenceAsync(Guid interviewId, QuestionnaireId questionnaireId, CancellationToken cancellationToken)
        {
            var text = $"INSERT INTO \"{tenantContext.Tenant.Name}\".\"{InterviewQuestionnaireReferenceTableName}\" ({InterviewColumnName}, {QuestionnaireColumnName})" +
                       $"           VALUES(@interviewId, @questionnaireId);";

            var command = this.tenantContext.Connection.CreateCommand();
            command.CommandText = text;
            AddParameter(command, "@interviewId", DbType.Guid, interviewId);
            AddParameter(command, "@questionnaireId", DbType.String, questionnaireId.Id);
            return command.ExecuteNonQueryAsync(cancellationToken);
        }

        public Task RemoveInterviewQuestionnaireReferenceAsync(Guid interviewId, CancellationToken cancellationToken)
        {
            var text = $"DELETE FROM \"{tenantContext.Tenant.Name}\".\"{InterviewQuestionnaireReferenceTableName}\" " +
                       $"      WHERE {InterviewColumnName} = @interviewId;";

            var command = this.tenantContext.Connection.CreateCommand();
            command.CommandText = text;
            AddParameter(command, "@interviewId", DbType.Guid, interviewId);
            return command.ExecuteNonQueryAsync(cancellationToken);
        }

        private static void AddParameter(DbCommand command, string name, DbType dbType, object value)
        {
            var parameter = command.CreateParameter();
            parameter.DbType = dbType;
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        private static bool doesExistTable = false;

        protected void EnshureTableExists()
        {
            if (doesExistTable) return;

            var command = $"CREATE TABLE IF NOT EXISTS \"{tenantContext.Tenant.Name}\".\"{InterviewQuestionnaireReferenceTableName}\"( " +
                          $"     {InterviewColumnName}  uuid PRIMARY KEY," +
                          $" {QuestionnaireColumnName}  text NOT NULL" +
                          $")";
            using (var sqlCommand = tenantContext.Connection.CreateCommand())
            {
                sqlCommand.CommandText = command;
                sqlCommand.ExecuteNonQuery();
            }

            doesExistTable = true;
        }
    }
}
