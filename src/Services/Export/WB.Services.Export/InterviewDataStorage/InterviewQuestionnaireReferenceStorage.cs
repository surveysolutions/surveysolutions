using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.InterviewDataStorage
{
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
            var commandText = $"SELECT {QuestionnaireColumnName} FROM \"{tenantContext.Tenant.Name}\".\"{InterviewQuestionnaireReferenceTableName}\"" +
                              $" WHERE {InterviewColumnName} = @interviewId;";
            using (var command = this.tenantContext.Connection.CreateCommand())
            {
                command.CommandText = commandText;
                AddParameter(command, "@interviewId", NpgsqlDbType.Uuid, interviewId);
                var questionnaireId = (await command.ExecuteScalarAsync(cancellationToken)) as string;
                return new QuestionnaireId(questionnaireId);
            }
        }

        public Task AddInterviewQuestionnaireReferenceAsync(Guid interviewId, QuestionnaireId questionnaireId, CancellationToken cancellationToken)
        {
            var text = $"INSERT INTO \"{tenantContext.Tenant.Name}\".\"{InterviewQuestionnaireReferenceTableName}\" ({InterviewColumnName}, {QuestionnaireColumnName})" +
                       $"           VALUES(@interviewId, @questionnaireId);";

            using (var command = this.tenantContext.Connection.CreateCommand())
            {
                command.CommandText = text;
                AddParameter(command, "@interviewId", NpgsqlDbType.Uuid, interviewId);
                AddParameter(command, "@questionnaireId", NpgsqlDbType.Text, questionnaireId.Id);
                return command.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        public Task RemoveInterviewQuestionnaireReferenceAsync(Guid interviewId, CancellationToken cancellationToken)
        {
            var text = $"DELETE FROM \"{tenantContext.Tenant.Name}\".\"{InterviewQuestionnaireReferenceTableName}\" " +
                       $"      WHERE {InterviewColumnName} = @interviewId;";

            using (var command = this.tenantContext.Connection.CreateCommand())
            {
                command.CommandText = text;
                AddParameter(command, "@interviewId", NpgsqlDbType.Uuid, interviewId);
                return command.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        private static void AddParameter(DbCommand command, string name, NpgsqlDbType dbType, object value)
        {
            var parameter = new NpgsqlParameter(name, dbType) { Value = value};
            command.Parameters.Add(parameter);
        }

        private bool doesExistTable = false;

        protected void EnshureTableExists()
        {
            if (doesExistTable) return;

            var command = $"CREATE TABLE IF NOT EXISTS \"{tenantContext.Tenant.Name}\".\"{InterviewQuestionnaireReferenceTableName}\"( " +
                          $"     {InterviewColumnName}  uuid PRIMARY KEY," +
                          $" {QuestionnaireColumnName}  text NOT NULL" +
                          $")";
            using (var sqlCommand = this.tenantContext.Connection.CreateCommand())
            {
                sqlCommand.CommandText = command;
                sqlCommand.ExecuteNonQuery();
            }

            doesExistTable = true;
        }
    }
}
