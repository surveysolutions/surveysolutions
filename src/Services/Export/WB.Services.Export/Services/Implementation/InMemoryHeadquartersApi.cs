using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Services.Implementation
{
    public class InMemoryHeadquartersApi : IHeadquartersApi
    {
        public Task<string> GetQuestionnaireAsync(QuestionnaireIdentity questionnaireId, Guid? translation,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<CategoryItem[]> GetCategoriesAsync(QuestionnaireIdentity questionnaireId, Guid categoryId, Guid? translation,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> GetPdfAsync(QuestionnaireIdentity questionnaireId, Guid? translation = null)
        {
            throw new NotImplementedException();
        }

        public Task<InterviewDiagnosticsInfo[]> GetInterviewDiagnosticsInfoBatchAsync(Guid[] interviewIds)
        {
            throw new NotImplementedException();
        }

        public Task<List<InterviewComment>> GetInterviewCommentsBatchAsync(Guid[] interviewIds)
        {
            throw new NotImplementedException();
        }

        public Task<List<InterviewAction>> GetInterviewSummariesBatchAsync(Guid[] interviewIds)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> GetInterviewImageAsync(Guid interviewId, string image)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> GetInterviewAudioAsync(Guid interviewId, string audio)
        {
            throw new NotImplementedException();
        }

        public Task<List<InterviewHistoryView>> GetInterviewsHistory(Guid[] id)
        {
            throw new NotImplementedException();
        }

        public Task<List<AudioAuditView>> GetAudioAuditInterviewsAsync(Guid[] interviewIds)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> GetAudioAuditAsync(Guid interviewId, string audio)
        {
            throw new NotImplementedException();
        }

        public Task<QuestionnaireAudioAuditView> DoesSupportAudioAuditAsync(QuestionnaireIdentity questionnaireId)
        {
            throw new NotImplementedException();
        }

        public Task<EventsFeed> GetInterviewEvents(long sequence, int pageSize = 500)
        {
            throw new NotImplementedException();
        }

        public Task<ServicesIntegration.Export.User> GetUserAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetExternalStorageAccessTokenByRefreshTokenAsync(ExternalStorageType type, string refreshToken)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> GetBackupAsync(QuestionnaireIdentity questionnaireId)
        {
            throw new NotImplementedException();
        }
    }
}
