using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.HttpServices.HttpClient;
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
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;
using CalendarEventPackageApiView = WB.Core.SharedKernels.DataCollection.WebApi.CalendarEventPackageApiView;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class OfflineSynchronizationService : IOfflineSynchronizationService
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

        public async Task<InterviewUploadState> GetInterviewUploadState(Guid interviewId, EventStreamSignatureTag eventStreamSignatureTag, CancellationToken token = default)
        {
            var result = await this.syncClient.SendAsync<GetInterviewUploadStateRequest, GetInterviewUploadStateResponse>(new GetInterviewUploadStateRequest
            {
                InterviewId = interviewId,
                Check = eventStreamSignatureTag
            }, token);

            return result.UploadState;
        }

        public Task UploadInterviewAsync(Guid interviewId, InterviewPackageApiView completedInterview,
            IProgress<TransferProgress> transferProgress, CancellationToken token = default)
        {
            var interviewKey = this.interviews.GetById(interviewId.FormatGuid())?.InterviewKey;
            return this.syncClient.SendAsync(new UploadInterviewRequest
            {
                Interview = completedInterview,
                InterviewKey = interviewKey
            },token, transferProgress);
        }
        
        
        public async Task<SyncInfoPackageResponse> GetSyncInfoPackageResponse(Guid interviewId, InterviewSyncInfoPackage interviewSyncInfoPackage,
            CancellationToken token = default)
        {
            var response = await syncClient.SendAsync<GetInterviewSyncInfoPackageRequest, InterviewSyncInfoPackageResponse>(
                new GetInterviewSyncInfoPackageRequest
                {
                    InterviewId = interviewId,
                    SyncInfoPackage = interviewSyncInfoPackage,
                },
                token);

            return response.SyncInfoPackageResponse;
        }

        public Task UploadInterviewImageAsync(Guid interviewId, string fileName, byte[] fileData,
            IProgress<TransferProgress> transferProgress,
            CancellationToken token = default)
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
            IProgress<TransferProgress> transferProgress, CancellationToken token = default)
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

        public Task UploadInterviewAudioAuditAsync(Guid interviewId, string fileName, string contentType, byte[] fileData,
            IProgress<TransferProgress> transferProgress, CancellationToken token = default)
        {
            return this.syncClient.SendAsync(new UploadInterviewAudioAuditRequest
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
            CancellationToken token = default)
        {
            var response = await syncClient.SendAsync<GetAttachmentContentsRequest, GetAttachmentContentsResponse>(
                new GetAttachmentContentsRequest {QuestionnaireIdentity = questionnaire}, token, transferProgress);

            return response.AttachmentContents;
        }

        public async Task<AttachmentContent> GetAttachmentContentAsync(string contentId,
            IProgress<TransferProgress> transferProgress, CancellationToken token = default)
        {
            var response = await syncClient.SendAsync<GetAttachmentContentRequest, GetAttachmentContentResponse>(
                new GetAttachmentContentRequest { ContentId = contentId }, token, transferProgress);

            return response.Content;
        }

        public async Task<List<QuestionnaireIdentity>> GetServerQuestionnairesAsync(CancellationToken token = default)
        {
            var localQuestionnaires = questionnaireAccessor.GetAllQuestionnaireIdentities();

            var response = await this.syncClient.SendAsync<GetQuestionnaireListRequest, GetQuestionnaireListResponse>(
                new GetQuestionnaireListRequest
                {
                    Questionnaires = localQuestionnaires
                }, token);

            return response.Questionnaires;
        }

        public async Task<List<QuestionnaireIdentity>> GetServerQuestionnairesPermittedToSwitchToWebModeAsync(CancellationToken cancellationToken)
        {
            var response = await this.syncClient.SendAsync<GetQuestionnairesWebModeRequest, GetQuestionnairesWebModeResponse>(
                new GetQuestionnairesWebModeRequest { }, cancellationToken);

            return response.Questionnaires;
        }

        public async Task<List<TranslationDto>> GetQuestionnaireTranslationAsync(
            QuestionnaireIdentity questionnaireIdentity, CancellationToken token = default)
        {
            var response = await this.syncClient
                .SendAsync<GetQuestionnaireTranslationRequest, GetQuestionnaireTranslationResponse>(
                    new GetQuestionnaireTranslationRequest
                    {
                        QuestionnaireIdentity = questionnaireIdentity
                    }, token);

            return response.Translations;
        }

        public async Task<CompanyLogoInfo> GetCompanyLogo(string storedClientEtag, CancellationToken token = default)
        {
            var response = await syncClient.SendAsync<GetCompanyLogoRequest, GetCompanyLogoResponse>(
                new GetCompanyLogoRequest
                {
                    Etag = storedClientEtag
                }, token);
            return response.LogoInfo;
        }

        public async Task<long?> SendSyncStatisticsAsync(SyncStatisticsApiView statistics, RestCredentials credentials, CancellationToken token = default)
        {
            await this.syncClient.SendAsync(
                new SyncStatisticsRequest(statistics, this.principal.CurrentUserIdentity.UserId), token);

            return this.settings.LastHqSyncTimestamp;
        }

        public Task SendUnexpectedExceptionAsync(UnexpectedExceptionApiView exception, CancellationToken token = default)
        {
            return this.syncClient.SendAsync(
                new SendUnexpectedExceptionRequest(exception, this.principal.CurrentUserIdentity.UserId), token);
        }

        public Task<List<MapView>> GetMapList(CancellationToken token = default)
        {
            return Task.FromResult(new List<MapView>());
        }

        public Task<RestStreamResult> GetMapContentStream(string mapName, CancellationToken token = default)
        {
            return Task.FromResult(new RestStreamResult());
        }

        public Task<InterviewerApiView> GetInterviewerAsync(RestCredentials? credentials = null, CancellationToken token = default)
        {
            throw new NotSupportedException("Offline mode is not support this method");
        }

        public async Task<Guid> GetCurrentSupervisor(RestCredentials credentials, CancellationToken token = default)
        {
            var response = await this.syncClient.SendAsync<SupervisorIdRequest, SupervisorIdResponse>(
                new SupervisorIdRequest(), token);

            return response.SupervisorId;
        }

        public Task<bool> IsAutoUpdateEnabledAsync(CancellationToken token = default) => Task.FromResult(true);

        public async Task<bool> AreNotificationsEnabledAsync(CancellationToken token = default)
        {
            var response = await this.syncClient.SendAsync<ApplicationSettingsRequest, ApplicationSettingsResponse>(
                new ApplicationSettingsRequest(), token);

            return response.NotificationsEnabled;
        }

        public async Task<List<ReusableCategoriesDto>> GetQuestionnaireReusableCategoriesAsync(QuestionnaireIdentity questionnaireIdentity,
            CancellationToken cancellationToken = default)
        {
            var response = await this.syncClient
                .SendAsync<GetQuestionnaireReusableCategoriesRequest, GetQuestionnaireReusableCategoriesResponse>(
                    new GetQuestionnaireReusableCategoriesRequest
                    {
                        QuestionnaireIdentity = questionnaireIdentity
                    }, cancellationToken);

            return response.Categories;
        }

        public async Task<RemoteTabletSettingsApiView> GetTabletSettings(CancellationToken cancellationToken)
        {
            var response = await syncClient.SendAsync<RemoteTabletSettingsRequest, RemoteTabletSettingsResponse>(
                new RemoteTabletSettingsRequest(), cancellationToken);

            return response.Settings;
        }

        public Task UploadCalendarEventAsync(Guid calendarEventId, CalendarEventPackageApiView calendarEventsPackage,
            IProgress<TransferProgress> transferProgress, CancellationToken token)
        {
            return syncClient.SendAsync<UploadCalendarEventRequest, OkResponse>(
                    new UploadCalendarEventRequest
                    {
                        InterviewerId = principal.CurrentUserIdentity.UserId,
                        CalendarEvent = calendarEventsPackage,
                    },
                    token);
        }
        
        public async Task<List<CommittedEvent>> GetCalendarEventStreamAsync(Guid calendarEventId, int? sequence, IProgress<TransferProgress> transferProgress, CancellationToken token)
        {
            var response = await
                syncClient.SendAsync<GetCalendarEventDetailsRequest, GetCalendarEventDetailsResponse>(
                    new GetCalendarEventDetailsRequest
                    {
                        CalendarEventId = calendarEventId,
                        Sequence = sequence
                    },
                    token);

            return response.Events;
        }

        public async Task<List<CalendarEventApiView>> GetCalendarEventsAsync(CancellationToken token = default)
        {
            var response = await
                syncClient.SendAsync<GetCalendarEventsRequest, GetCalendarEventsResponse>(
                    new GetCalendarEventsRequest { UserId = principal.CurrentUserIdentity.UserId },
                    token);

            return response.CalendarEvents;
        }

        public Task UploadAuditLogEntityAsync(AuditLogEntitiesApiView auditLogEntity, CancellationToken token = default)
        {
            return this.syncClient.SendAsync(new UploadAuditLogEntityRequest
            {
                AuditLogEntity = auditLogEntity
            }, token);
        }

        public Task<List<Guid>> CheckObsoleteInterviewsAsync(List<ObsoletePackageCheck> checks, CancellationToken token = default)
        {
            return Task.FromResult(new List<Guid>());
        }

        public async Task<AssignmentApiDocument> GetAssignmentAsync(int id, CancellationToken token = default)
        {
            var response = await
                syncClient.SendAsync<GetAssignmentRequest, GetAssignmentResponse>(new GetAssignmentRequest { Id = id },
                    token);

            return response.Assignment;
        }

        public Task LogAssignmentAsHandledAsync(int id, CancellationToken token = default)
        {
            return syncClient.SendAsync(new LogAssignmentAsHandledRequest { Id = id }, token);
        }

        public async Task<string> GetPublicKeyForEncryptionAsync(CancellationToken token = default)
        {
            var response = await syncClient.SendAsync<GetPublicKeyForEncryptionRequest, GetPublicKeyForEncryptionResponse>(
                new GetPublicKeyForEncryptionRequest(), token);

            return response.PublicKey;
        }

        public async Task<List<AssignmentApiView>> GetAssignmentsAsync(CancellationToken token = default)
        {
            var response = await
                syncClient.SendAsync<GetAssignmentsRequest, GetAssignmentsResponse>(new GetAssignmentsRequest { UserId = principal.CurrentUserIdentity.UserId },
                    token);

            return response.Assignments;
        }

        public Task<string> LoginAsync(LogonInfo logonInfo, RestCredentials credentials, CancellationToken token = default)
        {
            return Task.FromResult("offline sync token");
        }

        public Task<string> ChangePasswordAsync(ChangePasswordInfo info, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasCurrentUserDeviceAsync(RestCredentials? credentials = null, CancellationToken token = default)
        {
            return Task.FromResult(true);
        }

        public async Task<string> GetTenantId(RestCredentials? credentials = null, CancellationToken token = default)
        {
            var response = await syncClient.SendAsync<GetTenantIdRequest, GetTenantIdResponse>(
                new GetTenantIdRequest(), token);
            return response.TenantId;
        }

        public async Task CanSynchronizeAsync(RestCredentials? credentials = null, string? tenantId = null, CancellationToken token = default)
        {
            var interviewerIdentity = (IInterviewerUserIdentity)this.principal.CurrentUserIdentity;
            
            var request = new CanSynchronizeRequest(this.deviceSettings.GetApplicationVersionCode(), 
                interviewerIdentity.UserId,
                interviewerIdentity.SecurityStamp,
                settings.LastHqSyncTimestamp);

            var response = await this.syncClient.SendAsync<CanSynchronizeRequest, CanSynchronizeResponse>(request,  token);

            if (!response.CanSyncronize)
            {
                switch (response.Reason)
                {
                    case SyncDeclineReason.UnexpectedClientVersion :
                        var exception = new SynchronizationException(SynchronizationExceptionType.UpgradeRequired);
                        exception.Data["target-version"] = response.SupervisorVersion;
                        throw exception;
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

        public Task SendDeviceInfoAsync(DeviceInfoApiView info, CancellationToken token = default)
        {
            return this.syncClient.SendAsync(new UploadDeviceInfoRequest
            {
                UserId = principal.CurrentUserIdentity.UserId,
                DeviceInfo = info
            },token);
        }

        public Task LinkCurrentUserToDeviceAsync(RestCredentials? credentials = null, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        public async Task<byte[]> GetQuestionnaireAssemblyAsync(QuestionnaireIdentity questionnaire, IProgress<TransferProgress> transferProgress,
            CancellationToken token = default)
        {
            var response = await this.syncClient.SendAsync<GetQuestionnaireAssemblyRequest, GetQuestionnaireAssemblyResponse>(
                new GetQuestionnaireAssemblyRequest(questionnaire), token, transferProgress);
            return response.Content;
        }

        public async Task<QuestionnaireApiView> GetQuestionnaireAsync(QuestionnaireIdentity questionnaire, IProgress<TransferProgress> transferProgress,
            CancellationToken token = default)
        {
            var response = await this.syncClient.SendAsync<GetQuestionnaireRequest, GetQuestionnaireResponse>(
                new GetQuestionnaireRequest(questionnaire), token, transferProgress);
            return new QuestionnaireApiView
            {
                QuestionnaireDocument = response.QuestionnaireDocument
            };
        }

        public Task LogQuestionnaireAsSuccessfullyHandledAsync(QuestionnaireIdentity questionnaire)
        {
            return Task.CompletedTask;
        }

        public Task LogQuestionnaireAssemblyAsSuccessfullyHandledAsync(QuestionnaireIdentity questionnaire)
        {
            return Task.CompletedTask;
        }

        public async Task<byte[]> GetApplicationAsync(IProgress<TransferProgress>? transferProgress = null, CancellationToken token = default)
        {
            var response = await this.syncClient.SendChunkedAsync<GetInterviewerAppRequest, GetInterviewerAppResponse>(
                new GetInterviewerAppRequest(this.deviceSettings.GetApplicationVersionCode(), this.settings.ApplicationType), 
                token, transferProgress).ConfigureAwait(false);

            return response.Content;
        }

        public Task<byte[]?> GetApplicationPatchAsync(IProgress<TransferProgress>? transferProgress = null, CancellationToken token = default) 
            => Task.FromResult(default(byte[]));

        public async Task<int?> GetLatestApplicationVersionAsync(CancellationToken token = default)
        {
            var response = await this.syncClient.SendAsync<GetLatestApplicationVersionRequest, GetLatestApplicationVersionResponse>(
                new GetLatestApplicationVersionRequest(), token);
            return response.InterviewerApplicationVersion;
        }

        public async Task<List<InterviewApiView>> GetInterviewsAsync(CancellationToken token = default)
        {
            var response = await this.syncClient.SendAsync<GetInterviewsRequest, GetInterviewsResponse>(
                new GetInterviewsRequest(this.principal.CurrentUserIdentity.UserId), token);
            return response.Interviews;
        }

        public Task LogInterviewAsSuccessfullyHandledAsync(Guid interviewId)
        {
            return this.syncClient.SendAsync(new LogInterviewAsSuccessfullyHandledRequest(interviewId), CancellationToken.None);
        }

        public async Task<List<CommittedEvent>> GetInterviewDetailsAsync(Guid interviewId, IProgress<TransferProgress> transferProgress, CancellationToken token = default)
        {
            var response = await this.syncClient.SendAsync<GetInterviewDetailsRequest, GetInterviewDetailsResponse>(
                new GetInterviewDetailsRequest(interviewId), token, transferProgress);
            return response.Events;
        }

        public async Task<List<CommittedEvent>> GetInterviewDetailsAfterEventAsync(Guid interviewId, Guid eventId, IProgress<TransferProgress> transferProgress,
            CancellationToken token = default)
        {
            var response = await this.syncClient.SendAsync<GetInterviewDetailsAfterEventRequest, GetInterviewDetailsResponse>(
                new GetInterviewDetailsAfterEventRequest(interviewId, eventId), token, transferProgress);
            return response.Events;
        }
    }
}
