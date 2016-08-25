using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Platform;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class SynchronizationService : ISynchronizationService
    {
        private const string apiVersion = "v2";
        private const string interviewerApiUrl = "api/interviewer/";
        private readonly string devicesController = string.Concat(interviewerApiUrl, apiVersion, "/devices");
        private readonly string usersController = string.Concat(interviewerApiUrl, apiVersion, "/users");
        private readonly string interviewsController = string.Concat(interviewerApiUrl, apiVersion, "/interviews");
        private readonly string questionnairesController = string.Concat(interviewerApiUrl, apiVersion, "/questionnaires");
        private readonly string translationsController = string.Concat(interviewerApiUrl, apiVersion, "/translations");
        private readonly string attachmentContentController = string.Concat(interviewerApiUrl, apiVersion, "/attachments");

        private readonly IPrincipal principal;
        private readonly IRestService restService;
        private readonly IInterviewerSettings interviewerSettings;
        private readonly ISyncProtocolVersionProvider syncProtocolVersionProvider;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ILogger logger;

        private RestCredentials restCredentials => this.principal.CurrentUserIdentity == null
            ? null
            : new RestCredentials() { Login = this.principal.CurrentUserIdentity.Name, Password = this.principal.CurrentUserIdentity.Password };

        public SynchronizationService(
            IPrincipal principal, 
            IRestService restService,
            IInterviewerSettings interviewerSettings, 
            ISyncProtocolVersionProvider syncProtocolVersionProvider,
            IFileSystemAccessor fileSystemAccessor,
            ILogger logger)
        {
            this.principal = principal;
            this.restService = restService;
            this.interviewerSettings = interviewerSettings;
            this.syncProtocolVersionProvider = syncProtocolVersionProvider;
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;
        }

        #region [Interviewer Api]
        public async Task<InterviewerApiView> GetInterviewerAsync(RestCredentials credentials = null, CancellationToken? token = null)
        {
            return await this.TryGetRestResponseOrThrowAsync(async () => await this.restService.GetAsync<InterviewerApiView>(url: string.Concat(this.usersController, "/current"),
                credentials: credentials ?? this.restCredentials, token: token)).ConfigureAwait(false);

        }
        #endregion

        #region [Device Api]

        public async Task<bool> HasCurrentInterviewerDeviceAsync(RestCredentials credentials = null, CancellationToken? token = null)
        {
            return await this.TryGetRestResponseOrThrowAsync(async () => await this.restService.GetAsync<bool>(url: string.Concat(this.usersController, "/hasdevice"),
                credentials: credentials ?? this.restCredentials, token: token));
        }

        public async Task CanSynchronizeAsync(RestCredentials credentials = null, CancellationToken? token = null)
        {
            try
            {
                string url = string.Concat(interviewerApiUrl, "compatibility/", this.interviewerSettings.GetDeviceId(), "/",
                    this.syncProtocolVersionProvider.GetProtocolVersion());
                var respose = await this.TryGetRestResponseOrThrowAsync(async () => await this.restService.GetAsync(
                    url: url,
                    credentials: credentials ?? this.restCredentials, token: token));
                string response = await respose.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (response == null || response.Trim('"') != "449634775")
                {
                    throw new SynchronizationException(SynchronizationExceptionType.InvalidUrl, InterviewerUIResources.InvalidEndpoint);
                }
            }
            catch (SynchronizationException ex)
            {
                if ((ex.InnerException as RestException)?.StatusCode == HttpStatusCode.NotFound)
                    await this.OldCanSynchronizeAsync(credentials, token);
                else throw;
            }
        }

        [Obsolete("Since v5.10")]
        private async Task OldCanSynchronizeAsync(RestCredentials credentials = null, CancellationToken? token = null)
        {
            await this.TryGetRestResponseOrThrowAsync(async () => await this.restService.GetAsync(
                url: string.Concat(this.devicesController, "/current/" + this.interviewerSettings.GetDeviceId(), "/", this.syncProtocolVersionProvider.GetProtocolVersion()),
                credentials: credentials ?? this.restCredentials, token: token));
        }

        public async Task LinkCurrentInterviewerToDeviceAsync(RestCredentials credentials = null, CancellationToken? token = null)
        {
            await this.TryGetRestResponseOrThrowAsync(async () => await this.restService.PostAsync(
                url: string.Concat(this.devicesController, "/link/", this.interviewerSettings.GetDeviceId(), "/", this.syncProtocolVersionProvider.GetProtocolVersion()),
                credentials: credentials ?? this.restCredentials, token: token));
        }

        #endregion

        #region [Questionnaire Api]


        public async Task<byte[]> GetQuestionnaireAssemblyAsync(QuestionnaireIdentity questionnaire, Action<decimal, long, long> onDownloadProgressChanged,
            CancellationToken token)
        {
            return await this.TryGetRestResponseOrThrowAsync(async () =>
            {
                var restFile = await this.restService.DownloadFileAsync(
                    url: $"{this.questionnairesController}/{questionnaire.QuestionnaireId}/{questionnaire.Version}/assembly",
                    onDownloadProgressChanged: ToDownloadProgressChangedEvent(onDownloadProgressChanged),
                    token: token,
                    credentials: this.restCredentials).ConfigureAwait(false);
                return restFile.Content;
            }).ConfigureAwait(false);
        }



        public async Task<QuestionnaireApiView> GetQuestionnaireAsync(QuestionnaireIdentity questionnaire, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token)
        {
            var questionnaireContentVersion = this.interviewerSettings.GetSupportedQuestionnaireContentVersion().Major;
            return await this.TryGetRestResponseOrThrowAsync(async () => await this.restService.GetAsync<QuestionnaireApiView>(
                url: $"{this.questionnairesController}/{questionnaire.QuestionnaireId}/{questionnaire.Version}/{questionnaireContentVersion}",
                onDownloadProgressChanged: ToDownloadProgressChangedEvent(onDownloadProgressChanged),
                token: token,
                credentials: this.restCredentials)).ConfigureAwait(false);
        }

        public async Task<List<QuestionnaireIdentity>> GetCensusQuestionnairesAsync(CancellationToken token)
        {
            return await this.TryGetRestResponseOrThrowAsync(async () => await this.restService.GetAsync<List<QuestionnaireIdentity>>(
                url: string.Concat(this.questionnairesController, "/census"),
                credentials: this.restCredentials, token: token)).ConfigureAwait(false);
        }

        public async Task<List<QuestionnaireIdentity>> GetServerQuestionnairesAsync(CancellationToken cancellationToken)
        {
            return await this.TryGetRestResponseOrThrowAsync(async () => await this.restService.GetAsync<List<QuestionnaireIdentity>>(
              url: string.Concat(this.questionnairesController, "/list"),
              credentials: this.restCredentials, token: cancellationToken)).ConfigureAwait(false);
        }

        public async Task<List<TranslationDto>> GetQuestionnaireTranslationAsync(QuestionnaireIdentity questionnaireIdentity, CancellationToken cancellationToken)
        {
            var url = $"{this.translationsController}/{questionnaireIdentity}";

            return await this.TryGetRestResponseOrThrowAsync(async () => await this.restService.GetAsync<List<TranslationDto>>(
                url: url,
                credentials: this.restCredentials, token: cancellationToken)).ConfigureAwait(false);
        }

        public async Task LogQuestionnaireAsSuccessfullyHandledAsync(QuestionnaireIdentity questionnaire)
        {
            await this.TryGetRestResponseOrThrowAsync(async () => await this.restService.PostAsync(
                url: string.Concat(this.questionnairesController, "/", questionnaire.QuestionnaireId, "/", questionnaire.Version, "/logstate"),
                credentials: this.restCredentials)).ConfigureAwait(false);
        }

        public async Task LogQuestionnaireAssemblyAsSuccessfullyHandledAsync(QuestionnaireIdentity questionnaire)
        {
            await this.TryGetRestResponseOrThrowAsync(async () => await this.restService.PostAsync(
                url: string.Concat(this.questionnairesController, "/", questionnaire.QuestionnaireId, "/", questionnaire.Version, "/assembly/logstate"),
                credentials: this.restCredentials)).ConfigureAwait(false);
        }

        #endregion

        #region [Interview Api]

        public async Task<List<InterviewApiView>> GetInterviewsAsync(CancellationToken token)
        {
            return await this.TryGetRestResponseOrThrowAsync(async () =>
                await this.restService.GetAsync<List<InterviewApiView>>(url: this.interviewsController,
                    credentials: this.restCredentials, token: token)).ConfigureAwait(false);
        }

        public async Task LogInterviewAsSuccessfullyHandledAsync(Guid interviewId)
        {
            await this.TryGetRestResponseOrThrowAsync(async () => await this.restService.PostAsync(
                url: string.Concat(this.interviewsController, "/", interviewId, "/logstate"),
                credentials: this.restCredentials)).ConfigureAwait(false);
        }

        public async Task<InterviewerInterviewApiView> GetInterviewDetailsAsync(Guid interviewId, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token)
        {
            try
            {
                var interviewDetailsApiView = await this.TryGetRestResponseOrThrowAsync(async () => await this.restService.GetAsync<InterviewerInterviewApiView>(
                    url: string.Concat(this.interviewsController, "/", interviewId),
                    credentials: this.restCredentials,
                    onDownloadProgressChanged: ToDownloadProgressChangedEvent(onDownloadProgressChanged),
                    token: token)).ConfigureAwait(false);
                return interviewDetailsApiView;
            }
            catch (SynchronizationException exception)
            {
                var httpStatusCode = (exception.InnerException as RestException)?.StatusCode;
                if (httpStatusCode == HttpStatusCode.NotFound)
                    return null;

                this.logger.Error("Exception on download interview. ID:" + interviewId, exception);
                throw;
            }
        }

        public async Task UploadInterviewAsync(Guid interviewId, InterviewPackageApiView completedInterview, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token)
        {
            await this.TryGetRestResponseOrThrowAsync(async () => await this.restService.PostAsync(
                url: string.Concat(this.interviewsController, "/", interviewId),
                request: completedInterview,
                credentials: this.restCredentials,
                token: token)).ConfigureAwait(false);
        }

        public async Task UploadInterviewImageAsync(Guid interviewId, string fileName, byte[] fileData, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token)
        {
            await this.TryGetRestResponseOrThrowAsync(async () => await this.restService.PostAsync(
                url: string.Concat(this.interviewsController, "/", interviewId, "/image"),
                request: new PostFileRequest()
                {
                    InterviewId = interviewId,
                    FileName = fileName,
                    Data = Convert.ToBase64String(fileData)
                },
                credentials: this.restCredentials,
                token: token)).ConfigureAwait(false);
        }



        #endregion
        
        #region Attachments
        public async Task<List<string>> GetAttachmentContentsAsync(QuestionnaireIdentity questionnaire, 
            Action<decimal, long, long> onDownloadProgressChanged,
            CancellationToken token)
        {
            return await this.TryGetRestResponseOrThrowAsync(async () =>
                await this.restService.GetAsync<List<string>>(
                    url: $"{this.questionnairesController}/{questionnaire.QuestionnaireId}/{questionnaire.Version}/attachments",
                    credentials: this.restCredentials,
                    token: token)).ConfigureAwait(false);
        }

        public async Task<AttachmentContent> GetAttachmentContentAsync(string contentId, 
            Action<decimal, long, long> onDownloadProgressChanged, 
            CancellationToken token)
        {
            return await this.TryGetRestResponseOrThrowAsync(async () =>
            {
                var restFile = await this.restService.DownloadFileAsync(
                    url: $"{this.attachmentContentController}/{contentId}",
                    onDownloadProgressChanged: ToDownloadProgressChangedEvent(onDownloadProgressChanged),
                    token: token,
                    credentials: this.restCredentials).ConfigureAwait(false);

                var attachmentContent = new AttachmentContent()
                {
                    Content = restFile.Content,
                    ContentType = restFile.ContentType,
                    Id = restFile.ContentHash,
                    Size = restFile.ContentLength ?? restFile.Content.Length
                };
                return attachmentContent;
            }).ConfigureAwait(false);
        } 
        #endregion

        #region [Application Api]

        public async Task<byte[]> GetApplicationAsync(CancellationToken token)
        {
            return await this.TryGetRestResponseOrThrowAsync(async () =>
            {
                var restFile = await this.restService.DownloadFileAsync(url: interviewerApiUrl, token: token,
                    credentials: this.restCredentials).ConfigureAwait(false);

                return restFile.Content;
            });
        }

        public async Task<int?> GetLatestApplicationVersionAsync(CancellationToken token)
        {
            return await this.TryGetRestResponseOrThrowAsync(async () =>
                await this.restService.GetAsync<int?>(
                    url: string.Concat(interviewerApiUrl, "/latestversion"),
                    credentials: this.restCredentials, token: token).ConfigureAwait(false)).ConfigureAwait(false);
        }

        public async Task SendTabletInformationAsync(string filePath, CancellationToken token)
        {
            var tabletInformationHeaders = new Dictionary<string, string>()
            {
                { "DeviceId", this.interviewerSettings.GetDeviceId() },
            };

            await this.TryGetRestResponseOrThrowAsync(async () =>
            {
                using (var fileStream = this.fileSystemAccessor.ReadFile(filePath))
                {
                    await this.restService.SendStreamAsync(
                        stream: fileStream,
                        customHeaders: tabletInformationHeaders,
                        url: string.Concat(interviewerApiUrl, "/tabletInfo"),
                        credentials: this.restCredentials,
                        token: token).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);
        }

        #endregion

        private async Task TryGetRestResponseOrThrowAsync(Func<Task> restRequestTask)
        {
            if (restRequestTask == null) throw new ArgumentNullException(nameof(restRequestTask));

            try
            {
                await restRequestTask().ConfigureAwait(false);
            }
            catch (RestException ex)
            {
                throw this.CreateSynchronizationExceptionByRestException(ex);
            }
        }

        private async Task<T> TryGetRestResponseOrThrowAsync<T>(Func<Task<T>> restRequestTask)
        {
            if (restRequestTask == null) throw new ArgumentNullException(nameof(restRequestTask));

            try
            {
                return await restRequestTask().ConfigureAwait(false);
            }
            catch (RestException ex)
            {
                throw this.CreateSynchronizationExceptionByRestException(ex);
            }
        }

        private SynchronizationException CreateSynchronizationExceptionByRestException(RestException ex)
        {
            string exceptionMessage = InterviewerUIResources.UnexpectedException;
            SynchronizationExceptionType exceptionType = SynchronizationExceptionType.Unexpected;
            switch (ex.Type)
            {
                case RestExceptionType.RequestByTimeout:
                    exceptionMessage = InterviewerUIResources.RequestTimeout;
                    exceptionType = SynchronizationExceptionType.RequestByTimeout;
                    break;
                case RestExceptionType.RequestCanceledByUser:
                    exceptionMessage = InterviewerUIResources.RequestCanceledByUser;
                    exceptionType = SynchronizationExceptionType.RequestCanceledByUser;
                    break;
                case RestExceptionType.HostUnreachable:
                    exceptionMessage = InterviewerUIResources.HostUnreachable;
                    exceptionType = SynchronizationExceptionType.HostUnreachable;
                    break;
                case RestExceptionType.InvalidUrl:
                    exceptionMessage = InterviewerUIResources.InvalidEndpoint;
                    exceptionType = SynchronizationExceptionType.InvalidUrl;
                    break;
                case RestExceptionType.NoNetwork:
                    exceptionMessage = InterviewerUIResources.NoNetwork;
                    exceptionType = SynchronizationExceptionType.NoNetwork;
                    break;
                case RestExceptionType.Unexpected:
                    switch (ex.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            if (ex.Message.Contains("lock"))
                            {
                                exceptionMessage = InterviewerUIResources.AccountIsLockedOnServer;
                                exceptionType = SynchronizationExceptionType.UserLocked;
                            }
                            else if (ex.Message.Contains("not approved"))
                            {
                                exceptionMessage = InterviewerUIResources.AccountIsNotApprovedOnServer;
                                exceptionType = SynchronizationExceptionType.UserNotApproved;
                            }
                            else if (ex.Message.Contains("not have an interviewer role"))
                            {
                                exceptionMessage = InterviewerUIResources.AccountIsNotAnInterviewer;
                                exceptionType = SynchronizationExceptionType.UserIsNotInterviewer;
                            }
                            else
                            {
                                exceptionMessage = InterviewerUIResources.Unauthorized;
                                exceptionType = SynchronizationExceptionType.Unauthorized;
                            }
                            break;
                        case HttpStatusCode.ServiceUnavailable:
                            var isMaintenance = ex.Message.Contains("maintenance");

                            if (isMaintenance)
                            {
                                exceptionMessage = InterviewerUIResources.Maintenance;
                                exceptionType = SynchronizationExceptionType.Maintenance;
                            }
                            else
                            {
                                this.logger.Warn("Server error", ex);

                                exceptionMessage = InterviewerUIResources.ServiceUnavailable;
                                exceptionType = SynchronizationExceptionType.ServiceUnavailable;
                            }
                            break;
                        case HttpStatusCode.NotAcceptable:
                            exceptionMessage = InterviewerUIResources.NotSupportedServerSyncProtocolVersion;
                            exceptionType = SynchronizationExceptionType.NotSupportedServerSyncProtocolVersion;
                            break;
                        case HttpStatusCode.UpgradeRequired:
                            exceptionMessage = InterviewerUIResources.UpgradeRequired;
                            exceptionType = SynchronizationExceptionType.UpgradeRequired;
                            break;
                        case HttpStatusCode.BadRequest:
                        case HttpStatusCode.Redirect:
                            exceptionMessage = InterviewerUIResources.InvalidEndpoint;
                            exceptionType = SynchronizationExceptionType.InvalidUrl;
                            break;
                        case HttpStatusCode.NotFound:
                            this.logger.Warn("Server error", ex);
                            exceptionMessage = InterviewerUIResources.InvalidEndpoint;
                            exceptionType = SynchronizationExceptionType.InvalidUrl;
                            break;
                        case HttpStatusCode.InternalServerError:
                            this.logger.Warn("Server error", ex);

                            exceptionMessage = InterviewerUIResources.InternalServerError;
                            exceptionType = SynchronizationExceptionType.InternalServerError;
                            break;
                        case HttpStatusCode.Forbidden:
                            exceptionType = SynchronizationExceptionType.UserLinkedToAnotherDevice;
                            break;
                    }
                    break;
            }

            return new SynchronizationException(exceptionType, exceptionMessage, ex);
        }

        private static Action<DownloadProgressChangedEventArgs> ToDownloadProgressChangedEvent(Action<decimal, long, long> onDownloadProgressChanged)
        {
            Action<DownloadProgressChangedEventArgs> onDownloadProgressChangedInternal = (args) =>
            {
                onDownloadProgressChanged?.Invoke(args.ProgressPercentage, args.BytesReceived, args.TotalBytesToReceive ?? 0);
            };
            return onDownloadProgressChangedInternal;
        }
    }
}