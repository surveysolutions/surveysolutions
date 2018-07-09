using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public partial class OfflineSynchronizationService
    {
        private readonly IOfflineSyncClient syncClient;
        private readonly IPrincipal principal;

        public OfflineSynchronizationService(IOfflineSyncClient syncClient,
            IPrincipal principal)
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

        public Task<List<string>> GetAttachmentContentsAsync(QuestionnaireIdentity questionnaire,
            Action<decimal, long, long> onDownloadProgressChanged,
            CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<AttachmentContent> GetAttachmentContentAsync(string contentId,
            Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public Task<List<MapView>> GetMapList(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<RestStreamResult> GetMapContentStream(string mapName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Guid> GetCurrentSupervisor(CancellationToken token, RestCredentials credentials)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsAutoUpdateEnabledAsync(CancellationToken token)
        {
            return Task.FromResult(false);
        }

        public Task UploadAuditLogEntityAsync(AuditLogEntitiesApiView auditLogEntity,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<Guid>> CheckObsoleteInterviewsAsync(List<ObsoletePackageCheck> checks,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new List<Guid>());
        }
    }
}
