using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public partial class OfflineSynchronizationService
    {
        private readonly IOfflineSyncClient syncClient;
        private readonly IInterviewerPrincipal principal;

        public OfflineSynchronizationService(
            IOfflineSyncClient syncClient,
            IInterviewerPrincipal principal)
        {
            this.syncClient = syncClient;
            this.principal = principal;
        }

        public Task UploadInterviewAsync(Guid interviewId, InterviewPackageApiView completedInterview,
            Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task UploadInterviewImageAsync(Guid interviewId, string fileName, byte[] fileData,
            Action<decimal, long, long> onDownloadProgressChanged,
            CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task UploadInterviewAudioAsync(Guid interviewId, string fileName, string contentType, byte[] fileData,
            Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public async Task<List<string>> GetAttachmentContentsAsync(QuestionnaireIdentity questionnaire,
            Action<decimal, long, long> onDownloadProgressChanged,
            CancellationToken token)
        {
            var response = await syncClient.SendAsync<GetAttachmentContentsRequest, GetAttachmentContentsResponse>(
                new GetAttachmentContentsRequest {QuestionnaireIdentity = questionnaire}, token);

            return response.AttachmentContents;
        }

        public async Task<AttachmentContent> GetAttachmentContentAsync(string contentId,
            Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token)
        {
            var response = await syncClient.SendAsync<GetAttachmentContentRequest, GetAttachmentContentResponse>(
                new GetAttachmentContentRequest { ContentId = contentId }, token);

            return response.Content;
        }

        public async Task<List<QuestionnaireIdentity>> GetServerQuestionnairesAsync(CancellationToken cancellationToken)
        {
            var response = await this.syncClient.SendAsync<GetQuestionnaireList.Request, GetQuestionnaireList.Response>(
                   new GetQuestionnaireList.Request(), cancellationToken);

            return response.Questionnaires;
        }

        public async Task<List<TranslationDto>> GetQuestionnaireTranslationAsync(QuestionnaireIdentity questionnaireIdentity,
            CancellationToken cancellationToken)
        {
            var response = await this.syncClient
                .SendAsync<GetQuestionnaireTranslationRequest, GetQuestionnaireTranslationResponse>(
                    new GetQuestionnaireTranslationRequest
                    {
                        QuestionnaireIdentity = questionnaireIdentity
                    }, cancellationToken);

            return response.Translations;
        }

        public async Task<CompanyLogoInfo> GetCompanyLogo(string storedClientEtag, CancellationToken cancellationToken)
        {
            var response = await syncClient.SendAsync<GetCompanyLogoRequest, GetCompanyLogoResponse>(
                new GetCompanyLogoRequest
                {
                    Etag = storedClientEtag
                }, cancellationToken);
            return response.LogoInfo;
        }

        public Task SendSyncStatisticsAsync(SyncStatisticsApiView statistics,
            CancellationToken token,
            RestCredentials credentials)
        {
            throw new NotImplementedException();
        }

        public Task SendUnexpectedExceptionAsync(UnexpectedExceptionApiView exception, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public Task<List<MapView>> GetMapList(CancellationToken cancellationToken)
        {
            return Task.FromResult(new List<MapView>());
        }

        public Task<RestStreamResult> GetMapContentStream(string mapName, CancellationToken cancellationToken)
        {
            return Task.FromResult<RestStreamResult>(null);
        }

        public Task<Guid> GetCurrentSupervisor(CancellationToken token, RestCredentials credentials)
        {
            return Task.FromResult(this.principal.CurrentUserIdentity.SupervisorId);
        }

        public Task<bool> IsAutoUpdateEnabledAsync(CancellationToken token)
        {
            return Task.FromResult(false);
        }

        public Task UploadAuditLogEntityAsync(AuditLogEntitiesApiView auditLogEntity, CancellationToken cancellationToken)
        {
            return this.syncClient.SendAsync(new UploadAuditLogEntityRequest
            {
                AuditLogEntity = auditLogEntity
            }, cancellationToken);
        }

        public Task<List<Guid>> CheckObsoleteInterviewsAsync(List<ObsoletePackageCheck> checks,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new List<Guid>());
        }
    }
}
