using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernel.Structures.TabletInformation;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class SynchronizationService : ISynchronizationService
    {
        private const string interviewerApiUrl = "api/interviewer/v1";
        private readonly string devicesController = string.Concat(interviewerApiUrl, "/devices");
        private readonly string usersController = string.Concat(interviewerApiUrl, "/users");
        private readonly string interviewsController = string.Concat(interviewerApiUrl, "/interviews");
        private readonly string questionnairesController = string.Concat(interviewerApiUrl, "/questionnaires");

        private readonly IPrincipal principal;
        private readonly IRestService restService;
        private readonly IInterviewerSettings interviewerSettings;
        private readonly ISyncProtocolVersionProvider syncProtocolVersionProvider;

        private RestCredentials restCredentials
        {
            get
            {
                return this.principal.CurrentUserIdentity == null
                ? null
                : new RestCredentials() { Login = this.principal.CurrentUserIdentity.Name, Password = this.principal.CurrentUserIdentity.Password };
            }
        }

        public SynchronizationService(IPrincipal principal, IRestService restService,
            IInterviewerSettings interviewerSettings, ISyncProtocolVersionProvider syncProtocolVersionProvider)
        {
            this.principal = principal;
            this.restService = restService;
            this.interviewerSettings = interviewerSettings;
            this.syncProtocolVersionProvider = syncProtocolVersionProvider;
        }

        #region [User Api]

        public async Task<InterviewerApiView> GetCurrentInterviewerAsync(string login, string password, CancellationToken token)
        {
            return await this.restService.GetAsync<InterviewerApiView>(url: string.Concat(this.usersController, "/current"),
                        credentials: new RestCredentials() {Login = login, Password = password}, token: token);
        }

        public async Task<bool> HasCurrentInterviewerDeviceAsync(CancellationToken token, RestCredentials credentials = null)
        {
            return await this.restService.GetAsync<bool>(url: string.Concat(this.usersController, "/hasdevice"), 
                credentials: credentials ?? this.restCredentials, token: token);
        }

        #endregion

        #region [Device Api]

        public async Task<bool> IsDeviceLinkedToCurrentInterviewerAsync(CancellationToken token, RestCredentials credentials = null)
        {
            return await this.restService.GetAsync<bool>(url: string.Concat(this.devicesController, "/current/" + this.interviewerSettings.GetDeviceId()), 
                credentials: credentials ?? this.restCredentials, token: token);
        }

        public async Task LinkCurrentInterviewerToDeviceAsync(CancellationToken token, RestCredentials credentials = null)
        {
            await this.restService.PostAsync(
                url: string.Concat(this.devicesController, "/link/", this.interviewerSettings.GetDeviceId(), "/", this.syncProtocolVersionProvider.GetProtocolVersion()),
                credentials: credentials ?? this.restCredentials, token: token);
        }

        #endregion

        #region [Questionnaire Api]


        public async Task<byte[]> GetQuestionnaireAssemblyAsync(QuestionnaireIdentity questionnaire, Action<decimal, long, long> onDownloadProgressChanged,
            CancellationToken token)
        {
            return await this.restService.DownloadFileWithProgressAsync(
                url: string.Format("{0}/{1}/{2}/assembly", this.questionnairesController, questionnaire.QuestionnaireId, questionnaire.Version),
                onDownloadProgressChanged: ToDownloadProgressChangedEvent(onDownloadProgressChanged),
                token: token,
                credentials: this.restCredentials);
        }

        

        public async Task<QuestionnaireApiView> GetQuestionnaireAsync(QuestionnaireIdentity questionnaire, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token)
        {
            return await this.restService.GetWithProgressAsync<QuestionnaireApiView>(
                url: string.Format("{0}/{1}/{2}", this.questionnairesController, questionnaire.QuestionnaireId, questionnaire.Version),
                onDownloadProgressChanged: ToDownloadProgressChangedEvent(onDownloadProgressChanged),
                token: token,
                credentials: this.restCredentials);
        }

        public async Task<List<QuestionnaireIdentity>> GetCensusQuestionnairesAsync(CancellationToken token)
        {
            return await this.restService.GetAsync<List<QuestionnaireIdentity>>(
                url: string.Concat(this.questionnairesController, "/census"),
                credentials: this.restCredentials, token: token);
        }

        public async Task LogQuestionnaireAsSuccessfullyHandledAsync(QuestionnaireIdentity questionnaire, CancellationToken token)
        {
            await this.restService.PostAsync(
                url: string.Concat(this.questionnairesController, "/", questionnaire.QuestionnaireId, "/", questionnaire.Version, "/logstate"),
                credentials: this.restCredentials, token: token);
        }

        public async Task LogQuestionnaireAssemblyAsSuccessfullyHandledAsync(QuestionnaireIdentity questionnaire, CancellationToken token)
        {
            await this.restService.PostAsync(
                url: string.Concat(this.questionnairesController, "/", questionnaire.QuestionnaireId, "/", questionnaire.Version, "/assembly/logstate"),
                credentials: this.restCredentials, token: token);
        }

        #endregion

        #region [Interview Api]

        public async Task<List<InterviewApiView>> GetInterviewsAsync(CancellationToken token)
        {
            return await this.restService.GetAsync<List<InterviewApiView>>(url: this.interviewsController, credentials: this.restCredentials, token: token);
        }

        public async Task<InterviewPackagesApiView> GetInterviewPackagesAsync(string lastPackageId, CancellationToken token)
        {
            var interviewPackages = await this.restService.GetAsync<InterviewPackagesApiView>(
                url: string.Concat(this.interviewsController, "/packages/", lastPackageId), 
                credentials: this.restCredentials, token: token);

            interviewPackages.Interviews = interviewPackages.Interviews ?? new List<InterviewApiView>();
            interviewPackages.Packages = interviewPackages.Packages ?? new List<SynchronizationChunkMeta>();

            return interviewPackages;
        }

        public async Task LogPackageAsSuccessfullyHandledAsync(string packageId, CancellationToken token)
        {
            await this.restService.PostAsync(
                url: string.Concat(this.interviewsController, "/package/", packageId, "/logstate"), 
                credentials: this.restCredentials, token: token);
        }

        public async Task<InterviewSyncPackageDto> GetInterviewPackageAsync(string packageId, string previousSuccessfullyHandledPackageId, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token)
        {
            var url = string.Concat(this.interviewsController, "/package/", packageId);
            if (!string.IsNullOrEmpty(previousSuccessfullyHandledPackageId))
            {
                url = string.Concat(url, "/", previousSuccessfullyHandledPackageId);
            }
            return await this.restService.GetWithProgressAsync<InterviewSyncPackageDto>(
                url: url, 
                credentials: this.restCredentials,
                onDownloadProgressChanged: ToDownloadProgressChangedEvent(onDownloadProgressChanged),
                token: token);
        }

        public async Task UploadInterviewAsync(Guid interviewId, string content, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token)
        {
            await this.restService.PostAsync(
                url: string.Concat(this.interviewsController, "/package/", interviewId),
                request: content,
                credentials: this.restCredentials,
                token: token);
        }

        public async Task UploadInterviewImageAsync(Guid interviewId, string fileName, byte[] fileData, Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token)
        {
            await this.restService.PostAsync(
                url: string.Concat(this.interviewsController, "/", interviewId, "/image"),
                request: new PostFileRequest()
                {
                    InterviewId = interviewId,
                    FileName = fileName,
                    Data = Convert.ToBase64String(fileData)
                },
                credentials: this.restCredentials,
                token: token);
        }

        #endregion

        #region [Application Api]

        public async Task CheckInterviewerCompatibilityWithServerAsync(CancellationToken token)
        {
            await this.restService.GetAsync(url: string.Concat(interviewerApiUrl, "/compatibility/", this.syncProtocolVersionProvider.GetProtocolVersion()), credentials: this.restCredentials, token: token);
        }

        public async Task<byte[]> GetApplicationAsync(CancellationToken token)
        {
            return await this.restService.DownloadFileWithProgressAsync(url: interviewerApiUrl, token: token, credentials: this.restCredentials);
        }

        public async Task<int?> GetLatestApplicationVersionAsync(CancellationToken token)
        {
            return await this.restService.GetAsync<int?>(url: string.Concat(interviewerApiUrl, "/latestversion"), credentials: this.restCredentials, token: token);
        }

        public async Task SendTabletInformationAsync(string archive, CancellationToken token)
        {
            await this.restService.PostAsync(
                url: string.Concat(interviewerApiUrl, "/troubleshooting/tabletInfo"),
                credentials: this.restCredentials,
                request: new TabletInformationPackage()
                {
                    Content = archive,
                    AndroidId = this.interviewerSettings.GetDeviceId(),
                    ClientRegistrationId = this.interviewerSettings.GetClientRegistrationId().Value
                }, token: token);
        }

        #endregion

        private static Action<DownloadProgressChangedEventArgs> ToDownloadProgressChangedEvent(Action<decimal, long, long> onDownloadProgressChanged)
        {
            Action<DownloadProgressChangedEventArgs> onDownloadProgressChangedInternal = (args) =>
            {
                if (onDownloadProgressChanged != null)
                {
                    onDownloadProgressChanged(args.ProgressPercentage, args.BytesReceived, args.TotalBytesToReceive ?? 0);
                }
            };
            return onDownloadProgressChangedInternal;
        }
    }
}