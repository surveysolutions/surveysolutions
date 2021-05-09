using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Refit;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Services.Implementation
{
    /// <summary>
    /// Should not be injected
    /// </summary>
    public interface IRemoteHeadquartersApi
    {
        [Get("/api/export/v1/questionnaire/{id}")]
        Task<string> GetQuestionnaireAsync([AliasAs("id")] QuestionnaireIdentity questionnaireId, 
            Guid? translation,
            CancellationToken cancellationToken);

        [Get("/api/export/v1/questionnaire/{questionnaireId}/category/{categoryId}")]
        Task<CategoryItem[]> GetCategoriesAsync(QuestionnaireIdentity questionnaireId, Guid categoryId, Guid? translation, CancellationToken cancellationToken);

        [Get("/api/export/v1/questionnaire/{id}/pdf")]
        Task<HttpContent> GetPdfAsync([AliasAs("id")] QuestionnaireIdentity questionnaireId, Guid? translation = null);

        [Get("/api/export/v1/interview/batch/diagnosticsInfo")]
        Task<InterviewDiagnosticsInfo[]> GetInterviewDiagnosticsInfoBatchAsync([Query(CollectionFormat.Multi), AliasAs("id")] Guid[] interviewIds);

        [Get("/api/export/v1/interview/batch/commentaries")]
        Task<List<InterviewComment>> GetInterviewCommentsBatchAsync([Query(CollectionFormat.Multi), AliasAs("id")] Guid[] interviewIds);

        [Get("/api/export/v1/interview/batch/summaries")]
        Task<List<InterviewAction>> GetInterviewSummariesBatchAsync([Query(CollectionFormat.Multi), AliasAs("id")] Guid[] interviewIds);

        [Get("/api/export/v1/interview/{interviewId}/image/{image}")]
        Task<HttpContent> GetInterviewImageAsync(Guid interviewId, string image);
        
        [Get("/api/export/v1/interview/{interviewId}/audio/{audio}")]
        Task<HttpContent> GetInterviewAudioAsync(Guid interviewId, string audio);

        [Get("/api/export/v1/interview/batch/history")]
        Task<List<InterviewHistoryView>> GetInterviewsHistory([Query(CollectionFormat.Multi)] Guid[] id);

        [Get("/api/export/v1/interviews/batch/audioAudit")]
        Task<List<AudioAuditView>> GetAudioAuditInterviewsAsync([Query(CollectionFormat.Multi), AliasAs("id")] Guid[] interviewIds);

        [Get("/api/export/v1/interview/{interviewId}/audioAudit/{audio}")]
        Task<HttpContent> GetAudioAuditAsync(Guid interviewId, string audio);

        [Get("/api/export/v1/questionnaire/{id}/audioAudit")]
        Task<QuestionnaireAudioAuditView> DoesSupportAudioAuditAsync([AliasAs("id")] QuestionnaireIdentity questionnaireId);

        [Get("/api/export/v1/interview/events")]
        Task<EventsFeed> GetInterviewEvents([AliasAs("sequence")] long sequence, int pageSize = 500);

        [Get("/api/export/v1/user/{userId}")]
        Task<ServicesIntegration.Export.User> GetUserAsync(Guid userId);
        [Post("/api/export/v1/externalstorages/refreshtoken")]
        Task<string> GetExternalStorageAccessTokenByRefreshTokenAsync(ExternalStorageType type, string refreshToken);

        [Get("/api/export/v1/questionnaire/{id}/backup")]
        Task<HttpContent> GetBackupAsync([AliasAs("id")] QuestionnaireIdentity questionnaireId);
    }
}
