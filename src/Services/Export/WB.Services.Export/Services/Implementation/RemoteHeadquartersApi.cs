using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Services.Implementation
{
    public class RemoteHeadquartersApi : IHeadquartersApi
    {
        private readonly IRemoteHeadquartersApi remoteHeadquartersApi;

        public RemoteHeadquartersApi(IRemoteHeadquartersApi remoteHeadquartersApi)
        {
            this.remoteHeadquartersApi = remoteHeadquartersApi;
        }

        public Task<string> GetQuestionnaireAsync(QuestionnaireIdentity questionnaireId, Guid? translation,
            CancellationToken cancellationToken)
        {
            return this.remoteHeadquartersApi.GetQuestionnaireAsync(questionnaireId, translation, cancellationToken);
        }

        public Task<CategoryItem[]> GetCategoriesAsync(QuestionnaireIdentity questionnaireId, Guid categoryId, Guid? translation,
            CancellationToken cancellationToken)
        {
            return this.remoteHeadquartersApi.GetCategoriesAsync(questionnaireId, categoryId, translation,
                cancellationToken);
        }

        public async Task<Stream> GetPdfAsync(QuestionnaireIdentity questionnaireId, Guid? translation = null)
        {
            var response = await this.remoteHeadquartersApi.GetPdfAsync(questionnaireId, translation);
            return await response.ReadAsStreamAsync();
        }

        public Task<InterviewDiagnosticsInfo[]> GetInterviewDiagnosticsInfoBatchAsync(Guid[] interviewIds)
        {
            return this.remoteHeadquartersApi.GetInterviewDiagnosticsInfoBatchAsync(interviewIds);
        }

        public Task<List<InterviewComment>> GetInterviewCommentsBatchAsync(Guid[] interviewIds)
        {
            return this.remoteHeadquartersApi.GetInterviewCommentsBatchAsync(interviewIds);
        }

        public Task<List<InterviewAction>> GetInterviewSummariesBatchAsync(Guid[] interviewIds)
        {
            return this.remoteHeadquartersApi.GetInterviewSummariesBatchAsync(interviewIds);
        }

        public async Task<Stream> GetInterviewImageAsync(Guid interviewId, string image)
        {
            var response = await this.remoteHeadquartersApi.GetInterviewImageAsync(interviewId, image);
            return await response.ReadAsStreamAsync();
        }

        public async Task<Stream> GetInterviewAudioAsync(Guid interviewId, string audio)
        {
            var response = await this.remoteHeadquartersApi.GetInterviewAudioAsync(interviewId, audio);
            return await response.ReadAsStreamAsync();
        }

        public Task<List<InterviewHistoryView>> GetInterviewsHistory(Guid[] id)
        {
            return this.remoteHeadquartersApi.GetInterviewsHistory(id);
        }

        public Task<List<AudioAuditView>> GetAudioAuditInterviewsAsync(Guid[] interviewIds)
        {
            return this.remoteHeadquartersApi.GetAudioAuditInterviewsAsync(interviewIds);
        }

        public async Task<Stream> GetAudioAuditAsync(Guid interviewId, string audio)
        {
            var response = await this.remoteHeadquartersApi.GetAudioAuditAsync(interviewId, audio);
            return await response.ReadAsStreamAsync();
        }

        public Task<QuestionnaireAudioAuditView> DoesSupportAudioAuditAsync(QuestionnaireIdentity questionnaireId)
        {
            return this.remoteHeadquartersApi.DoesSupportAudioAuditAsync(questionnaireId);
        }

        public Task<EventsFeed> GetInterviewEvents(long sequence, int pageSize = 500)
        {
            return this.remoteHeadquartersApi.GetInterviewEvents(sequence, pageSize);
        }

        public Task<ServicesIntegration.Export.User> GetUserAsync(Guid userId)
        {
            return this.remoteHeadquartersApi.GetUserAsync(userId);
        }

        public Task<string> GetExternalStorageAccessTokenByRefreshTokenAsync(ExternalStorageType type, string refreshToken)
        {
            return this.remoteHeadquartersApi.GetExternalStorageAccessTokenByRefreshTokenAsync(type, refreshToken);
        }

        public async Task<Stream> GetBackupAsync(QuestionnaireIdentity questionnaireId)
        {
            var response = await this.remoteHeadquartersApi.GetBackupAsync(questionnaireId);
            return await response.ReadAsStreamAsync();
        }
    }
}
