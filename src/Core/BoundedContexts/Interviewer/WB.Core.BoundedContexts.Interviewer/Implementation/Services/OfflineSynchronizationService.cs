using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    [ExcludeFromCodeCoverage]
    public class OfflineSynchronizationService : IInterviewerSynchronizationService
    {
        private readonly IOfflineSyncClient syncClient;
        private readonly IInterviewerPrincipal principal;
        private readonly IInterviewerQuestionnaireAccessor questionnaireAccessor;
        private readonly IPlainStorage<InterviewView> interviews;
        private readonly IEnumeratorSettings settings;
        private readonly IDeviceSettings deviceSettings;

        public OfflineSynchronizationService(
            IOfflineSyncClient syncClient,
            IInterviewerPrincipal principal,
            IInterviewerQuestionnaireAccessor questionnaireAccessor,
            IPlainStorage<InterviewView> interviews, 
            IEnumeratorSettings settings,
            IDeviceSettings deviceSettings)
        {
            this.syncClient = syncClient;
            this.principal = principal;
            this.questionnaireAccessor = questionnaireAccessor;
            this.interviews = interviews;
            this.settings = settings;
            this.deviceSettings = deviceSettings;
        }

        public async Task<InterviewUploadState> GetInterviewUploadState(Guid interviewId, EventStreamSignatureTag eventStreamSignatureTag, CancellationToken cancellationToken)
        {
            var result = await this.syncClient.SendAsync<GetInterviewUploadStateRequest, GetInterviewUploadStateResponse>(new GetInterviewUploadStateRequest
            {
                InterviewId = interviewId,
                Check = eventStreamSignatureTag
            }, cancellationToken);

            return result.UploadState;
        }

        public Task UploadInterviewAsync(Guid interviewId, InterviewPackageApiView completedInterview,
            IProgress<TransferProgress> transferProgress, CancellationToken token)
        {
            var interviewKey = this.interviews.GetById(interviewId.FormatGuid())?.InterviewKey;
            return this.syncClient.SendAsync(new UploadInterviewRequest
            {
                Interview = completedInterview,
                InterviewKey = interviewKey
            },token, transferProgress);
        }

        public Task UploadInterviewImageAsync(Guid interviewId, string fileName, byte[] fileData,
            IProgress<TransferProgress> transferProgress,
            CancellationToken token)
        {
            return this.syncClient.SendAsync(new UploadInterviewImageRequest
            {
                InterviewImage = new PostFileApiView()
                {
                    FileName = fileName,
                    InterviewId = interviewId,
                    Data = Convert.ToBase64String(fileData)
                },
            }, token, transferProgress);
        }

        public Task UploadInterviewAudioAsync(Guid interviewId, string fileName, string contentType, byte[] fileData,
            IProgress<TransferProgress> transferProgress, CancellationToken token)
        {
            return this.syncClient.SendAsync(new UploadInterviewAudioRequest
            {
                InterviewAudio = new PostFileApiView()
                {
                    FileName = fileName,
                    InterviewId = interviewId,
                    ContentType = contentType,
                    Data = Convert.ToBase64String(fileData)
                },
            }, token, transferProgress);
        }

        public async Task<List<string>> GetAttachmentContentsAsync(QuestionnaireIdentity questionnaire,
            IProgress<TransferProgress> transferProgress,
            CancellationToken token)
        {
            var response = await syncClient.SendAsync<GetAttachmentContentsRequest, GetAttachmentContentsResponse>(
                new GetAttachmentContentsRequest {QuestionnaireIdentity = questionnaire}, token, transferProgress);

            return response.AttachmentContents;
        }

        public async Task<AttachmentContent> GetAttachmentContentAsync(string contentId,
            IProgress<TransferProgress> transferProgress, CancellationToken token)
        {
            var response = await syncClient.SendAsync<GetAttachmentContentRequest, GetAttachmentContentResponse>(
                new GetAttachmentContentRequest { ContentId = contentId }, token, transferProgress);

            return response.Content;
        }

        public async Task<List<QuestionnaireIdentity>> GetServerQuestionnairesAsync(CancellationToken cancellationToken)
        {
            var localQuestionnaires = questionnaireAccessor.GetAllQuestionnaireIdentities();

            var response = await this.syncClient.SendAsync<GetQuestionnaireListRequest, GetQuestionnaireListResponse>(
                new GetQuestionnaireListRequest
                {
                    Questionnaires = localQuestionnaires
                }, cancellationToken);

            return response.Questionnaires;
        }

        public async Task<List<TranslationDto>> GetQuestionnaireTranslationAsync(
            QuestionnaireIdentity questionnaireIdentity, CancellationToken cancellationToken)
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

        public async Task<long?> SendSyncStatisticsAsync(SyncStatisticsApiView statistics,
            CancellationToken token,
            RestCredentials credentials)
        {
            await this.syncClient.SendAsync(new SyncStatisticsRequest(statistics, this.principal.CurrentUserIdentity.UserId),
                token);

            return this.settings.LastHqSyncTimestamp;
        }

        public Task SendUnexpectedExceptionAsync(UnexpectedExceptionApiView exception, CancellationToken token)
        {
            return this.syncClient.SendAsync(new SendUnexpectedExceptionRequest(exception, this.principal.CurrentUserIdentity.UserId), token);
        }

        public Task<List<MapView>> GetMapList(CancellationToken cancellationToken)
        {
            return Task.FromResult(new List<MapView>());
        }

        public Task<RestStreamResult> GetMapContentStream(string mapName, CancellationToken cancellationToken)
        {
            return Task.FromResult<RestStreamResult>(null);
        }

        public Task<InterviewerApiView> GetInterviewerAsync(RestCredentials credentials = null, CancellationToken? token = null)
        {
            throw new NotSupportedException("Offline mode is not support this method");
        }

        public async Task<Guid> GetCurrentSupervisor(CancellationToken cancellationToken, RestCredentials credentials)
        {
            var response = await this.syncClient.SendAsync<SupervisorIdRequest, SupervisorIdResponse>(
                new SupervisorIdRequest(), cancellationToken);

            return response.SupervisorId;
        }

        public Task<bool> IsAutoUpdateEnabledAsync(CancellationToken token) => Task.FromResult(true);

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

        public async Task<AssignmentApiDocument> GetAssignmentAsync(int id, CancellationToken cancellationToken)
        {
            var response = await
                syncClient.SendAsync<GetAssignmentRequest, GetAssignmentResponse>(new GetAssignmentRequest { Id = id },
                    cancellationToken);

            return response.Assignment;
        }

        public Task LogAssignmentAsHandledAsync(int id, CancellationToken cancellationToken)
        {
            return syncClient.SendAsync(new LogAssignmentAsHandledRequest { Id = id }, cancellationToken);
        }

        public async Task<string> GetPublicKeyForEncryptionAsync(CancellationToken cancellationToken)
        {
            var response = await syncClient.SendAsync<GetPublicKeyForEncryptionRequest, GetPublicKeyForEncryptionResponse>(
                new GetPublicKeyForEncryptionRequest(), cancellationToken);

            return response.PublicKey;
        }

        public async Task<List<AssignmentApiView>> GetAssignmentsAsync(CancellationToken cancellationToken)
        {
            var response = await
                syncClient.SendAsync<GetAssignmentsRequest, GetAssignmentsResponse>(new GetAssignmentsRequest { UserId = principal.CurrentUserIdentity.UserId },
                    cancellationToken);

            return response.Assignments;
        }

        public Task<string> LoginAsync(LogonInfo logonInfo, RestCredentials credentials, CancellationToken? token = null)
        {
            return Task.FromResult("offline sync token");
        }

        public Task<bool> HasCurrentUserDeviceAsync(RestCredentials credentials = null, CancellationToken? token = null)
        {
            return Task.FromResult(true);
        }

        public async Task CanSynchronizeAsync(RestCredentials credentials = null, CancellationToken? token = null)
        {
            var request = new CanSynchronizeRequest(this.deviceSettings.GetApplicationVersionCode(), 
                this.principal.CurrentUserIdentity.UserId,
                this.principal.CurrentUserIdentity.SecurityStamp,
                settings.LastHqSyncTimestamp);

            var response = await this.syncClient.SendAsync<CanSynchronizeRequest, CanSynchronizeResponse>(request, 
                token ?? CancellationToken.None);

            if (!response.CanSyncronize)
            {
                switch (response.Reason)
                {
                    case SyncDeclineReason.UnexpectedClientVersion :
                        throw new SynchronizationException(SynchronizationExceptionType.UpgradeRequired);
                    case SyncDeclineReason.NotATeamMember :
                        throw new SynchronizationException(SynchronizationExceptionType.InterviewerFromDifferentTeam);
                    case SyncDeclineReason.InvalidPassword:
                        throw new SynchronizationException(SynchronizationExceptionType.Unauthorized);
                    case SyncDeclineReason.UserIsLocked:
                        throw new SynchronizationException(SynchronizationExceptionType.UserLocked);
                    case SyncDeclineReason.SupervisorRequireOnlineSync:
                        throw new SynchronizationException(SynchronizationExceptionType.SupervisorRequireOnlineSync);
                    default:
                        throw new SynchronizationException(SynchronizationExceptionType.Unexpected);
                }
            }
        }

        public Task SendDeviceInfoAsync(DeviceInfoApiView info, CancellationToken? token = null)
        {
            return this.syncClient.SendAsync(new UploadDeviceInfoRequest
            {
                UserId = principal.CurrentUserIdentity.UserId,
                DeviceInfo = info
            },token ?? CancellationToken.None);
        }

        public Task LinkCurrentUserToDeviceAsync(RestCredentials credentials = null, CancellationToken? token = null)
        {
            return Task.CompletedTask;
        }

        public async Task<byte[]> GetQuestionnaireAssemblyAsync(QuestionnaireIdentity questionnaire, IProgress<TransferProgress> transferProgress,
            CancellationToken token)
        {
            var response = await this.syncClient.SendAsync<GetQuestionnaireAssemblyRequest, GetQuestionnaireAssemblyResponse>(
                new GetQuestionnaireAssemblyRequest(questionnaire), token, transferProgress);
            return response.Content;
        }

        public async Task<QuestionnaireApiView> GetQuestionnaireAsync(QuestionnaireIdentity questionnaire, IProgress<TransferProgress> transferProgress,
            CancellationToken token)
        {
            var response = await this.syncClient.SendAsync<GetQuestionnaireRequest, GetQuestionnaireResponse>(
                new GetQuestionnaireRequest(questionnaire), token, transferProgress);
            return new QuestionnaireApiView
            {
                QuestionnaireDocument = response.QuestionnaireDocument
            };
        }

        public Task<List<QuestionnaireIdentity>> GetCensusQuestionnairesAsync(CancellationToken token)
        {
            return Task.FromResult(new List<QuestionnaireIdentity>());
        }

        public Task LogQuestionnaireAsSuccessfullyHandledAsync(QuestionnaireIdentity questionnaire)
        {
            return Task.CompletedTask;
        }

        public Task LogQuestionnaireAssemblyAsSuccessfullyHandledAsync(QuestionnaireIdentity questionnaire)
        {
            return Task.CompletedTask;
        }

        public async Task<byte[]> GetApplicationAsync(CancellationToken token, IProgress<TransferProgress> transferProgress = null)
        {
            var response = await this.syncClient.SendChunkedAsync<GetInterviewerAppRequest, GetInterviewerAppResponse>(
                new GetInterviewerAppRequest(this.deviceSettings.GetApplicationVersionCode(), this.settings.ApplicationType), 
                token, transferProgress).ConfigureAwait(false);

            return response.Content;
        }

        public Task<byte[]> GetApplicationPatchAsync(CancellationToken token, IProgress<TransferProgress> transferProgress = null) 
            => Task.FromResult<byte[]>(null);

        public async Task<int?> GetLatestApplicationVersionAsync(CancellationToken token)
        {
            var response = await this.syncClient.SendAsync<GetLatestApplicationVersionRequest, GetLatestApplicationVersionResponse>(
                new GetLatestApplicationVersionRequest(), token);
            return response.InterviewerApplicationVersion;
        }

        public async Task<List<InterviewApiView>> GetInterviewsAsync(CancellationToken token)
        {
            var response = await this.syncClient.SendAsync<GetInterviewsRequest, GetInterviewsResponse>(
                new GetInterviewsRequest(this.principal.CurrentUserIdentity.UserId), token);
            return response.Interviews;
        }

        public Task LogInterviewAsSuccessfullyHandledAsync(Guid interviewId)
        {
            return this.syncClient.SendAsync(new LogInterviewAsSuccessfullyHandledRequest(interviewId), CancellationToken.None);
        }

        public async Task<List<CommittedEvent>> GetInterviewDetailsAsync(Guid interviewId, IProgress<TransferProgress> transferProgress, CancellationToken token)
        {
            var response = await this.syncClient.SendAsync<GetInterviewDetailsRequest, GetInterviewDetailsResponse>(
                new GetInterviewDetailsRequest(interviewId), token, transferProgress);
            return response.Events;
        }
    }
}
