using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WB.ServicesIntegration.Export
{
       /// <summary>
    /// Should not be injected
    /// </summary>
    public interface IHeadquartersApi
    {
        Task<string> GetQuestionnaireAsync(QuestionnaireIdentity questionnaireId, 
            Guid? translation,
            CancellationToken cancellationToken);

        Task<CategoryItem[]> GetCategoriesAsync(QuestionnaireIdentity questionnaireId, Guid categoryId, Guid? translation, CancellationToken cancellationToken);

        Task<Stream> GetPdfAsync(QuestionnaireIdentity questionnaireId, Guid? translation = null);

        Task<InterviewDiagnosticsInfo[]> GetInterviewDiagnosticsInfoBatchAsync(Guid[] interviewIds);

        Task<List<InterviewComment>> GetInterviewCommentsBatchAsync(Guid[] interviewIds);

        Task<List<InterviewAction>> GetInterviewSummariesBatchAsync(Guid[] interviewIds);

        Task<Stream> GetInterviewImageAsync(Guid interviewId, string image);
        
        Task<Stream> GetInterviewAudioAsync(Guid interviewId, string audio);

        Task<List<InterviewHistoryView>> GetInterviewsHistory(Guid[] id);

        Task<List<AudioAuditView>> GetAudioAuditInterviewsAsync(Guid[] interviewIds);

        Task<Stream> GetAudioAuditAsync(Guid interviewId, string audio);

        Task<QuestionnaireAudioAuditView> DoesSupportAudioAuditAsync(QuestionnaireIdentity questionnaireId);

        Task<EventsFeed> GetInterviewEvents(long sequence, int pageSize = 500);

        Task<User> GetUserAsync(Guid userId);

        Task<string> GetExternalStorageAccessTokenByRefreshTokenAsync(ExternalStorageType type, string refreshToken);

        Task<Stream> GetBackupAsync(QuestionnaireIdentity questionnaireId);
    }
}
