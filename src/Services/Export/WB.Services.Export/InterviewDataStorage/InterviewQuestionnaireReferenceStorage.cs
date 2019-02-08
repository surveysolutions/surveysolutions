using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.InterviewDataStorage
{
    public class InterviewQuestionnaireReferenceStorage : IInterviewQuestionnaireReferenceStorage
    {
        private readonly ITenantContext tenantContext;
        private const string InterviewQuestionnaireReferenceTableName = "interview__questionnaire";
        private const string InterviewColumnName = "interview_id";
        private const string QuestionnaireColumnName = "questionnaire_id";

        public InterviewQuestionnaireReferenceStorage(ITenantContext tenantContext)
        {
            this.tenantContext = tenantContext;
            EnsureTableExists();
        }

        public async Task<QuestionnaireId> GetQuestionnaireIdByInterviewIdAsync(Guid interviewId, CancellationToken cancellationToken)
        {
            var commandText = $"SELECT {QuestionnaireColumnName} FROM \"{tenantContext.Tenant.Name}\".\"{InterviewQuestionnaireReferenceTableName}\"" +
                              $" WHERE {InterviewColumnName} = @interviewId;";

            var questionnaireId = await this.tenantContext.Connection.ExecuteScalarAsync<string>(commandText, new
            {
                interviewId = interviewId
            });
            return new QuestionnaireId(questionnaireId);
        }

        public Task AddInterviewQuestionnaireReferenceAsync(Guid interviewId, QuestionnaireId questionnaireId, CancellationToken cancellationToken)
        {
            var text = $"INSERT INTO \"{tenantContext.Tenant.Name}\".\"{InterviewQuestionnaireReferenceTableName}\" ({InterviewColumnName}, {QuestionnaireColumnName})" +
                       $"           VALUES(@interviewId, @questionnaireId);";

            return this.tenantContext.Connection.ExecuteAsync(text, new
            {
                interviewId = interviewId,
                questionnaireId = questionnaireId.Id
            });
        }

        public Task RemoveInterviewQuestionnaireReferenceAsync(Guid interviewId, CancellationToken cancellationToken)
        {
            var text = $"DELETE FROM \"{tenantContext.Tenant.Name}\".\"{InterviewQuestionnaireReferenceTableName}\" " +
                       $"      WHERE {InterviewColumnName} = @interviewId;";

            return this.tenantContext.Connection.ExecuteAsync(text, new {interviewId = interviewId});
        }

        private void EnsureTableExists()
        {
            var command = $"CREATE TABLE IF NOT EXISTS \"{tenantContext.Tenant.Name}\".\"{InterviewQuestionnaireReferenceTableName}\"( " +
                          $"     {InterviewColumnName}  uuid PRIMARY KEY," +
                          $" {QuestionnaireColumnName}  text NOT NULL" +
                          $")";

            this.tenantContext.Connection.Execute(command);
        }
    }
}
