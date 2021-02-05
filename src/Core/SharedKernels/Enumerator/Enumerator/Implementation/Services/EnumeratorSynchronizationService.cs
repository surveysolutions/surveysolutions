using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.Infrastructure.HttpServices.Services;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public abstract class EnumeratorSynchronizationService : ISynchronizationService
    {
        protected abstract string ApiVersion { get; }
        protected abstract string ApiUrl { get; }
        protected string EnumeratorApiVersion => "v1";
        protected string EnumeratorApiUrl => "api/enumerator/";
        protected string EnumeratorApplicationUrl => string.Concat(EnumeratorApiUrl, EnumeratorApiVersion);
        protected string ApplicationUrl => string.Concat(ApiUrl, ApiVersion);
        protected string AuditLogController => string.Concat(ApplicationUrl, "/auditlog");
        protected string DevicesController => string.Concat(ApplicationUrl, "/devices");
        protected string UsersController => string.Concat(ApplicationUrl, "/users");
        protected virtual string InterviewsController => string.Concat(ApplicationUrl, "/interviews");
        protected virtual string CalendarEventsController => string.Concat(ApplicationUrl, "/calendarevents");
        protected string QuestionnairesController => string.Concat(ApplicationUrl, "/questionnaires");
        protected string AssignmentsController => string.Concat(ApplicationUrl, "/assignments");
        protected string TranslationsController => string.Concat(ApplicationUrl, "/translations");
        protected string AttachmentContentController => string.Concat(ApplicationUrl, "/attachments");
        protected string ReusableCategoriesController => string.Concat(EnumeratorApplicationUrl, "/categories");
        protected string LogoUrl => string.Concat(ApplicationUrl, "/companyLogo");
        protected string TenantIdUrl => string.Concat(ApplicationUrl, "/tenantId");
        protected string AutoUpdateUrl => string.Concat(ApplicationUrl, "/autoupdate");
        protected string NotificationsUrl => string.Concat(ApplicationUrl, "/notifications");
        protected string PublicKeyForEncryptionUrl => string.Concat(ApplicationUrl, "/encryption-key");
        protected string RemoteTabletSettingsUrl => string.Concat(ApplicationUrl, "/tabletsettings");
        protected string MapsController => string.Concat(ApplicationUrl, "/maps"); 

        private readonly IPrincipal principal;
        protected readonly IRestService restService;
        protected readonly IDeviceSettings deviceSettings;
        protected readonly ISyncProtocolVersionProvider syncProtocolVersionProvider;
        protected readonly ICheckVersionUriProvider checkVersionUriProvider;
        protected readonly ILogger logger;
        protected readonly IEnumeratorSettings enumeratorSettings;
        
        protected RestCredentials restCredentials => this.principal.CurrentUserIdentity == null
            ? null
            : new RestCredentials {
                Login = this.principal.CurrentUserIdentity.Name,
                Token = this.principal.CurrentUserIdentity.Token,
                Workspace = this.principal.CurrentUserIdentity.Workspace
            };

        protected EnumeratorSynchronizationService(
            IPrincipal principal, 
            IRestService restService,
            IDeviceSettings deviceSettings, 
            ISyncProtocolVersionProvider syncProtocolVersionProvider,
            ICheckVersionUriProvider checkVersionUriProvider,
            ILogger logger,
            IEnumeratorSettings enumeratorSettings)
        {
            this.principal = principal;
            this.restService = restService;
            this.deviceSettings = deviceSettings;
            this.syncProtocolVersionProvider = syncProtocolVersionProvider;
            this.checkVersionUriProvider = checkVersionUriProvider;
            this.logger = logger;
            this.enumeratorSettings = enumeratorSettings;
        }

        #region [User Api]

        public async Task<string> LoginAsync(LogonInfo logonInfo, RestCredentials credentials, CancellationToken token = default)
        {
            try
            {
                var authToken = await this.TryGetRestResponseOrThrowAsync(() => this.restService.PostAsync<string>(
                    url: string.Concat(this.UsersController, "/login"),
                    request: logonInfo,
                    credentials: credentials,
                    token: token)).ConfigureAwait(false);

                return authToken;
            }
            catch (RestException ex)
            {
                throw ex.ToSynchronizationException();
            }
        }
        
        public async Task<string> ChangePasswordAsync(ChangePasswordInfo info, CancellationToken token = default)
        {
            try
            {
                var authToken = await this.TryGetRestResponseOrThrowAsync(() => this.restService.PostAsync<string>(
                    url: string.Concat(this.UsersController, "/changePassword"),
                    request: info,
                    token: token)).ConfigureAwait(false);

                return authToken;
            }
            catch (RestException ex)
            {
                throw ex.ToSynchronizationException();
            }
        }

        public Task<bool> IsAutoUpdateEnabledAsync(CancellationToken token = default)
            => this.TryGetRestResponseOrThrowAsync(
                () => this.restService.GetAsync<bool>(url: AutoUpdateUrl,
                    credentials: this.restCredentials, token: token));

        public Task<bool> AreNotificationsEnabledAsync(CancellationToken token = default)
            => this.TryGetRestResponseOrThrowAsync(() =>
            {
                return this.restService.GetAsync<bool>(url: NotificationsUrl, credentials: this.restCredentials,
                    token: token);
            });

        public Task UploadAuditLogEntityAsync(AuditLogEntitiesApiView entities, CancellationToken token = default)
        {
            return this.TryGetRestResponseOrThrowAsync(() => this.restService.PostAsync(
                url: $"{this.AuditLogController}",
                request: entities,
                credentials: this.restCredentials,
                token: token));
        }

        public Task<List<Guid>> CheckObsoleteInterviewsAsync(List<ObsoletePackageCheck> checks, CancellationToken token = default)
        {
            return this.TryGetRestResponseOrThrowAsync(() => this.restService.PostAsync<List<Guid>>(
                url: string.Concat(InterviewsController, "/CheckObsoleteInterviews"),
                request: checks,
                credentials: this.restCredentials,
                token: token));
        }

        #endregion

        #region AssignmentsApi

        public Task<List<AssignmentApiView>> GetAssignmentsAsync(CancellationToken token = default)
        {
            var response = this.TryGetRestResponseOrThrowAsync(() => this.restService.GetAsync<List<AssignmentApiView>>(
                url: this.AssignmentsController, credentials: this.restCredentials, token: token));

            return response;
        }

        public Task LogAssignmentAsHandledAsync(int id, CancellationToken token = default)
        {
            var response = this.TryGetRestResponseOrThrowAsync(() => this.restService.PostAsync(
                url: $"{this.AssignmentsController}/{id}/Received", credentials: this.restCredentials, token: token));

            return response;
        }

        public Task<string> GetPublicKeyForEncryptionAsync(CancellationToken token = default)
            => this.TryGetRestResponseOrThrowAsync(() => this.restService.GetAsync<string>(
                url: this.PublicKeyForEncryptionUrl, credentials: this.restCredentials, token: token));

        public Task<AssignmentApiDocument> GetAssignmentAsync(int id, CancellationToken token = default)
        {
            var response = this.TryGetRestResponseOrThrowAsync(() => this.restService.GetAsync<AssignmentApiDocument>(
                url: $"{this.AssignmentsController}/{id}", credentials: this.restCredentials, token: token));

            return response;
        }

        #endregion

        #region [Device Api]

        public Task<bool> HasCurrentUserDeviceAsync(RestCredentials credentials = null, CancellationToken token = default)
        {
            return this.TryGetRestResponseOrThrowAsync(() => this.restService.GetAsync<bool>(url: string.Concat(this.UsersController, "/hasdevice"),
                credentials: credentials ?? this.restCredentials, token: token));
        }

        public async Task<string> GetTenantId(RestCredentials credentials = null, CancellationToken token = default)
        {
            var response = await this.TryGetRestResponseOrThrowAsync(() => this.restService.GetAsync<TenantIdApiView>(
                url: TenantIdUrl, credentials: credentials ?? this.restCredentials, token: token)).ConfigureAwait(false); ;
            return response.TenantId;
        }

        public async Task CanSynchronizeAsync(RestCredentials credentials = null, string tenantId = null, CancellationToken token = default)
        {
            string url = string.Concat(ApiUrl, "compatibility/", this.deviceSettings.GetDeviceId(), "/",
                this.syncProtocolVersionProvider.GetProtocolVersion());

            if (tenantId != null)
            {
                url += "?tenantId=" + WebUtility.UrlEncode(tenantId);
            }

            var response = await this.TryGetRestResponseOrThrowAsync(() => this.restService.GetAsync<string>(
                url: url, credentials: credentials ?? this.restCredentials, token: token)).ConfigureAwait(false);

            if (response == null || response.Trim('"') != CanSynchronizeValidResponse)
            {
                throw new SynchronizationException(SynchronizationExceptionType.InvalidUrl, EnumeratorUIResources.InvalidEndpoint);
            }
        }

        protected abstract string CanSynchronizeValidResponse { get; }

        public Task SendDeviceInfoAsync(DeviceInfoApiView info, CancellationToken token = default)
        {
            return this.TryGetRestResponseOrThrowAsync(() => this.restService.PostAsync(
                url: $"{this.DevicesController}/info",
                request: info,
                credentials: this.restCredentials,
                token: token));
        }

        public Task<long?> SendSyncStatisticsAsync(SyncStatisticsApiView statistics, RestCredentials credentials, CancellationToken token = default)
        {
            return this.TryGetRestResponseOrThrowAsync(() => this.restService.PostAsync<long?>(
                url: $"{this.DevicesController}/statistics",
                request: statistics,
                credentials: this.restCredentials,
                token: token));
        }

        public Task SendUnexpectedExceptionAsync(UnexpectedExceptionApiView exception, CancellationToken token = default)
        {
            return this.TryGetRestResponseOrThrowAsync(() => this.restService.PostAsync(
                url: $"{this.DevicesController}/exception",
                request: exception,
                credentials: this.restCredentials,
                token: token));
        }

        public Task LinkCurrentUserToDeviceAsync(RestCredentials credentials = null, CancellationToken token = default)
        {
            return this.TryGetRestResponseOrThrowAsync(() => this.restService.PostAsync(
                url: string.Concat(this.DevicesController, "/link/", this.deviceSettings.GetDeviceId(), "/", this.syncProtocolVersionProvider.GetProtocolVersion()),
                credentials: credentials ?? this.restCredentials, token: token));
        }

        #endregion

        #region [Questionnaire Api]
        
        public Task<byte[]> GetQuestionnaireAssemblyAsync(QuestionnaireIdentity questionnaire, IProgress<TransferProgress> transferProgress,
            CancellationToken token = default)
        {
            return this.TryGetRestResponseOrThrowAsync(async () =>
            {
                var restFile = await this.restService.DownloadFileAsync(
                    url: $"{this.QuestionnairesController}/{questionnaire.QuestionnaireId}/{questionnaire.Version}/assembly",
                    transferProgress: transferProgress,
                    token: token,
                    credentials: this.restCredentials).ConfigureAwait(false);
                return restFile.Content;
            });
        }

        public Task<QuestionnaireApiView> GetQuestionnaireAsync(QuestionnaireIdentity questionnaire, IProgress<TransferProgress> transferProgress, CancellationToken token = default)
        {
            var questionnaireContentVersion = this.enumeratorSettings.GetSupportedQuestionnaireContentVersion().Major;

            return this.TryGetRestResponseOrThrowAsync(() => this.restService.GetAsync<QuestionnaireApiView>(
                url: $"{this.QuestionnairesController}/{questionnaire.QuestionnaireId}/{questionnaire.Version}/{questionnaireContentVersion}",
                transferProgress: transferProgress,
                token: token,
                credentials: this.restCredentials));
        }

        public Task<List<QuestionnaireIdentity>> GetServerQuestionnairesAsync(CancellationToken cancellationToken)
        {
            return this.TryGetRestResponseOrThrowAsync(() => this.restService.GetAsync<List<QuestionnaireIdentity>>(
                url: string.Concat(this.QuestionnairesController, "/list"), credentials: this.restCredentials, token: cancellationToken));
        }
      
        public Task<List<MapView>> GetMapList(CancellationToken cancellationToken)
        {
            return  this.TryGetRestResponseOrThrowAsync(() => this.restService.GetAsync<List<MapView>>(
                url: this.MapsController, token: cancellationToken, credentials: this.restCredentials));
        }
        
        public Task<RestStreamResult> GetMapContentStream(string mapName, CancellationToken cancellationToken)
        {
            return this.TryGetRestResponseOrThrowAsync(async () => 
                await this.restService.GetResponseStreamAsync(
                    url: $"{this.MapsController}/details",
                    queryString: new {id = WebUtility.UrlEncode(mapName)},
                    token: cancellationToken,
                    credentials: this.restCredentials).ConfigureAwait(false));
        }

        public Task<List<TranslationDto>> GetQuestionnaireTranslationAsync(QuestionnaireIdentity questionnaireIdentity, CancellationToken cancellationToken)
        {
            var url = $"{this.TranslationsController}/{questionnaireIdentity}";

            return this.TryGetRestResponseOrThrowAsync(() =>  this.restService.GetAsync<List<TranslationDto>>(
                url: url,
                credentials: this.restCredentials, token: cancellationToken));
        }

        public Task<List<ReusableCategoriesDto>> GetQuestionnaireReusableCategoriesAsync(QuestionnaireIdentity questionnaireIdentity,
            CancellationToken cancellationToken)
        {
            var url = $"{this.ReusableCategoriesController}/{questionnaireIdentity}";

            return this.TryGetRestResponseOrThrowAsync(() => this.restService.GetAsync<List<ReusableCategoriesDto>>(
                url: url, credentials: this.restCredentials, token: cancellationToken));
        }

        public Task<RemoteTabletSettingsApiView> GetTabletSettings(CancellationToken token = default)
            => this.TryGetRestResponseOrThrowAsync(() => 
                this.restService.GetAsync<RemoteTabletSettingsApiView>(
                    url: RemoteTabletSettingsUrl, 
                    credentials: this.restCredentials,
                    token: token
               ));


        public Task LogQuestionnaireAsSuccessfullyHandledAsync(QuestionnaireIdentity questionnaire)
        {
            return this.TryGetRestResponseOrThrowAsync(async () => await this.restService.PostAsync(
                url: string.Concat(this.QuestionnairesController, "/", questionnaire.QuestionnaireId, "/", questionnaire.Version, "/logstate"),
                credentials: this.restCredentials));
        }

        public Task LogQuestionnaireAssemblyAsSuccessfullyHandledAsync(QuestionnaireIdentity questionnaire)
        {
            return this.TryGetRestResponseOrThrowAsync(() => this.restService.PostAsync(
                url: string.Concat(this.QuestionnairesController, "/", questionnaire.QuestionnaireId, "/", questionnaire.Version, "/assembly/logstate"),
                credentials: this.restCredentials));
        }
        
        #endregion

        #region [Interview Api]

        public Task<List<InterviewApiView>> GetInterviewsAsync(CancellationToken token = default)
        {
            return this.TryGetRestResponseOrThrowAsync(() =>
                this.restService.GetAsync<List<InterviewApiView>>(url: this.InterviewsController,
                    credentials: this.restCredentials, token: token));
        }

        public Task LogInterviewAsSuccessfullyHandledAsync(Guid interviewId)
        {
            return this.TryGetRestResponseOrThrowAsync(() => this.restService.PostAsync(
                url: string.Concat(this.InterviewsController, "/", interviewId, "/logstate"),
                credentials: this.restCredentials));
        }

        public Task<List<CommittedEvent>> GetInterviewDetailsAsync(Guid interviewId, IProgress<TransferProgress> transferProgress, CancellationToken token = default)
        {
            try
            {
                return this.TryGetRestResponseOrThrowAsync(
                    () =>  this.restService.GetAsync<List<CommittedEvent>>(
                        url: string.Concat(this.InterviewsController, "/", interviewId),
                        credentials: this.restCredentials,
                        transferProgress: transferProgress,
                        token: token));
            }
            catch (SynchronizationException exception)
            {
                var httpStatusCode = (exception.InnerException as RestException)?.StatusCode;
                if (httpStatusCode == HttpStatusCode.NotFound)
                    return Task.FromResult<List<CommittedEvent>>(null);

                this.logger.Error("Exception on download interview. ID:" + interviewId, exception);
                throw;
            }
        }

        public Task<List<CommittedEvent>> GetInterviewDetailsAfterEventAsync(Guid interviewId, Guid eventId, IProgress<TransferProgress> transferProgress, CancellationToken token = default)
        {
            try
            {
                return this.TryGetRestResponseOrThrowAsync(
                    () =>  this.restService.GetAsync<List<CommittedEvent>>(
                        url: string.Concat(this.InterviewsController, "/", interviewId, "/", eventId),
                        credentials: this.restCredentials,
                        transferProgress: transferProgress,
                        token: token));
            }
            catch (SynchronizationException exception)
            {
                var httpStatusCode = (exception.InnerException as RestException)?.StatusCode;
                if (httpStatusCode == HttpStatusCode.NotFound)
                    return Task.FromResult<List<CommittedEvent>>(null);

                this.logger.Error("Exception on download interview. ID:" + interviewId, exception);
                throw;
            }
        }

        public Task<InterviewUploadState> GetInterviewUploadState(Guid interviewId, EventStreamSignatureTag eventStreamSignatureTag, CancellationToken cancellationToken)
        {
            try
            {
                return this.TryGetRestResponseOrThrowAsync(
                    () => this.restService.PostAsync<InterviewUploadState>(
                        url: string.Concat(this.InterviewsController,"/", interviewId, "/getInterviewUploadState"),
                        credentials: this.restCredentials, 
                        token: cancellationToken,
                        request: eventStreamSignatureTag));
            }
            catch (SynchronizationException exception)
            {
                var httpStatusCode = (exception.InnerException as RestException)?.StatusCode;
                if (httpStatusCode == HttpStatusCode.NotFound)
                    return Task.FromResult(new InterviewUploadState());

                this.logger.Error("Exception on download interview. ID:" + interviewId, exception);
                throw;
            }
        }

        public Task UploadInterviewAsync(Guid interviewId, InterviewPackageApiView completedInterview, IProgress<TransferProgress> transferProgress, CancellationToken token = default)
        {
            return this.TryGetRestResponseOrThrowAsync(() => this.restService.PostAsync(
                url: string.Concat(this.InterviewsController, "/", interviewId),
                request: completedInterview,
                credentials: this.restCredentials,
                token: token));
        }

        public Task UploadInterviewImageAsync(Guid interviewId, string fileName, byte[] fileData, IProgress<TransferProgress> transferProgress, CancellationToken token = default)
        {
            return this.TryGetRestResponseOrThrowAsync(() => this.restService.PostAsync(
                string.Concat(this.InterviewsController, "/", interviewId, "/image"),
                new PostFileRequest
                {
                    InterviewId = interviewId,
                    FileName = fileName,
                    Data = Convert.ToBase64String(fileData)
                },
                this.restCredentials,
                token));
        }

        public Task UploadInterviewAudioAsync(Guid interviewId, string fileName, string contentType, byte[] fileData, IProgress<TransferProgress> transferProgress,
            CancellationToken token = default)
        {
            return this.TryGetRestResponseOrThrowAsync(() => this.restService.PostAsync(
                url: string.Concat(this.InterviewsController, "/", interviewId, "/audio"),
                request: new PostFileRequest
                {
                    InterviewId = interviewId,
                    FileName = fileName,
                    ContentType = contentType,
                    Data = Convert.ToBase64String(fileData)
                },
                credentials: this.restCredentials,
                token: token));
        }

        public Task UploadInterviewAudioAuditAsync(Guid interviewId, string fileName, string contentType, byte[] fileData,
            IProgress<TransferProgress> transferProgress, CancellationToken token = default)
        {
            return this.TryGetRestResponseOrThrowAsync(() => this.restService.PostAsync(
                url: string.Concat(this.InterviewsController, "/", interviewId, "/audioaudit"),
                request: new PostFileRequest
                {
                    InterviewId = interviewId,
                    FileName = fileName,
                    ContentType = contentType,
                    Data = Convert.ToBase64String(fileData)
                },
                credentials: this.restCredentials,
                token: token));
        }

        #endregion
        
        #region Attachments
        public Task<List<string>> GetAttachmentContentsAsync(QuestionnaireIdentity questionnaire, 
            IProgress<TransferProgress> transferProgress,
            CancellationToken token = default)
        {
            return this.TryGetRestResponseOrThrowAsync(() =>
                this.restService.GetAsync<List<string>>(
                    url: $"{this.QuestionnairesController}/{questionnaire.QuestionnaireId}/{questionnaire.Version}/attachments",
                    credentials: this.restCredentials,
                    token: token));
        }

        public Task<AttachmentContent> GetAttachmentContentAsync(string contentId, 
            IProgress<TransferProgress> transferProgress, 
            CancellationToken token = default)
        {
            return this.TryGetRestResponseOrThrowAsync(async () =>
            {
                var restFile = await this.restService.DownloadFileAsync(
                    url: $"{this.AttachmentContentController}/{contentId}",
                    transferProgress: transferProgress,
                    token: token,
                    credentials: this.restCredentials).ConfigureAwait(false);

                var attachmentContent = new AttachmentContent()
                {
                    Content = restFile.Content,
                    ContentType = restFile.ContentType,
                    Id = restFile.ContentHash.Trim('"'),
                    Size = restFile.ContentLength ?? restFile.Content.Length
                };
                return attachmentContent;
            });
        }
        #endregion

        #region [Application Api]

        public async Task<byte[]> GetApplicationAsync(IProgress<TransferProgress> transferProgress = null, CancellationToken token = default)
        {
            try
            {
                var restFile = await this.restService.DownloadFileAsync(
                    url: this.checkVersionUriProvider.CheckVersionUrl, token: token,
                    credentials: this.restCredentials,
                    transferProgress: transferProgress).ConfigureAwait(false); ;

                return restFile.Content;
            }
            catch (RestException ex)
            {
                var newException = ex.ToSynchronizationException();
                throw newException;
            }
        }

        public async Task<byte[]> GetApplicationPatchAsync(IProgress<TransferProgress> transferProgress, CancellationToken token = default)
        {
            var interviewerPatchApiUrl = $"{this.checkVersionUriProvider.CheckVersionUrl}patch/{this.deviceSettings.GetApplicationVersionCode()}";
         
            var restFile = await this.restService.DownloadFileAsync(url: interviewerPatchApiUrl,
                    token: token,
                    credentials: this.restCredentials,
                    transferProgress: transferProgress).ConfigureAwait(false); ;

            return restFile.Content;
        }

        public Task<int?> GetLatestApplicationVersionAsync(CancellationToken token = default)
        {
            return this.TryGetRestResponseOrThrowAsync(async () =>
            {
                try
                {
                    return await this.restService.GetAsync<int?>(
                        url: string.Concat(this.checkVersionUriProvider.CheckVersionUrl, "latestversion"),
                        credentials: this.restCredentials, token: token).ConfigureAwait(false);
                }
                catch (RestException rest) when (rest.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
            });
        }

        #endregion

        #region [CalendarEvent API]
        
        public Task UploadCalendarEventAsync(Guid calendarEventId, 
            CalendarEventPackageApiView completedInterview, 
            IProgress<TransferProgress> transferProgress, CancellationToken token = default)
        {
            return this.TryGetRestResponseOrThrowAsync(() => this.restService.PostAsync(
                url: string.Concat(CalendarEventsController, "/", calendarEventId),
                request: completedInterview,
                credentials: this.restCredentials,
                token: token));
        }

        public Task<List<CommittedEvent>> GetCalendarEventStreamAsync(Guid calendarEventId, 
            int? sequence, 
            IProgress<TransferProgress> transferProgress, CancellationToken token = default)
        {
            try
            {
                return this.TryGetRestResponseOrThrowAsync(
                    () =>  this.restService.GetAsync<List<CommittedEvent>>(
                        url: string.Concat(CalendarEventsController, "/", calendarEventId),
                        credentials: this.restCredentials,
                        transferProgress: transferProgress,
                        token: token));
            }
            catch (SynchronizationException exception)
            {
                this.logger.Error("Exception on download interview. ID:" + CalendarEventsController, exception);
                throw;
            }
        }
        
        public Task<List<CalendarEventApiView>> GetCalendarEventsAsync(CancellationToken token = default)
        {
            return this.TryGetRestResponseOrThrowAsync(() =>
                this.restService.GetAsync<List<CalendarEventApiView>>(url: this.CalendarEventsController,
                    credentials: this.restCredentials, token: token));
        }
        
        #endregion

        protected async Task TryGetRestResponseOrThrowAsync(Func<Task> restRequestTask)
        {
            if (restRequestTask == null) throw new ArgumentNullException(nameof(restRequestTask));

            try
            {
                await restRequestTask().ConfigureAwait(false);
            }
            catch (RestException ex)
            {
                throw ex.ToSynchronizationException();
            }
        }

        protected async Task<T> TryGetRestResponseOrThrowAsync<T>(Func<Task<T>> restRequestTask)
        {
            if (restRequestTask == null) throw new ArgumentNullException(nameof(restRequestTask));

            try
            {
                return await restRequestTask().ConfigureAwait(false);
            }
            catch (RestException ex)
            {
                var newException = ex.ToSynchronizationException();
                throw newException;
            }
        }

        public async Task<CompanyLogoInfo> GetCompanyLogo(string storedClientEtag, CancellationToken cancellationToken)
        {
            var response = await this.TryGetRestResponseOrThrowAsync(() => this.restService.DownloadFileAsync(
                url: this.LogoUrl,
                credentials: this.restCredentials,
                customHeaders: !string.IsNullOrEmpty(storedClientEtag) ? new Dictionary<string, string> {{"If-None-Match", storedClientEtag }} : null,
                token: cancellationToken)).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NoContent)
                return new CompanyLogoInfo {HasCustomLogo = false};
            if (response.StatusCode == HttpStatusCode.NotModified)
                return new CompanyLogoInfo {LogoNeedsToBeUpdated = false, HasCustomLogo = true};

            return new CompanyLogoInfo
            {
                HasCustomLogo = true,
                LogoNeedsToBeUpdated = true,
                Logo = response.Content,
                Etag = response.ContentHash
            };
        }
    }
}
