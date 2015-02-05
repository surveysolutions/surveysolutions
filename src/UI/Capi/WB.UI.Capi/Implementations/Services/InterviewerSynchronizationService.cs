using System;
using System.Threading.Tasks;

using WB.Core.BoundedContexts.Capi.Implementation.Authorization;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Capi.Properties;
using WB.UI.Capi.Settings;

namespace WB.UI.Capi.Implementations.Services
{
    internal class InterviewerSynchronizationService : ISynchronizationService
    {
        private readonly IRestService restService;
        private readonly IInterviewerSettings interviewerSettings;
        private readonly ISyncProtocolVersionProvider syncProtocolVersionProvider;

        public InterviewerSynchronizationService(IRestService restService, IInterviewerSettings interviewerSettings, ISyncProtocolVersionProvider syncProtocolVersionProvider)
        {
            this.restService = restService;
            this.interviewerSettings = interviewerSettings;
            this.syncProtocolVersionProvider = syncProtocolVersionProvider;
        }

        public async Task<bool> CheckExpectedDeviceAsync(SyncCredentials credentials)
        {
            var deviceId = this.interviewerSettings.GetDeviceId();
            var isThisExpectedDevice = await this.restService.GetAsync<bool>(
                url: "api/InterviewerSync/CheckExpectedDevice",
                credentials: new RestCredentials {Login = credentials.Login, Password = credentials.Password},
                queryString: new { deviceId });

            return isThisExpectedDevice;
        }

        public async Task<Guid> HandshakeAsync(SyncCredentials credentials, bool shouldThisDeviceBeLinkedToUser = false)
        {
            var package = await this.restService.PostAsync<HandshakePackage>(
                url: "api/InterviewerSync/GetHandshakePackage",
                credentials: new RestCredentials {Login = credentials.Login, Password = credentials.Password},
                request: new HandshakePackageRequest {
                    ClientId = this.interviewerSettings.GetInstallationId(),
                    Version = this.syncProtocolVersionProvider.GetProtocolVersion(),
                    ClientRegistrationId = this.interviewerSettings.GetClientRegistrationId(),
                    AndroidId = this.interviewerSettings.GetDeviceId(),
                    ShouldDeviceBeLinkedToUser = shouldThisDeviceBeLinkedToUser
                });

            return package.ClientRegistrationKey;
        }

        public async Task<SyncItemsMetaContainer> GetChunksAsync(SyncCredentials credentials, string lastKnownPackageId)
        {
            var syncItemsMetaContainer = await restService.PostAsync<SyncItemsMetaContainer>(
                url: "api/InterviewerSync/GetARKeys",
                credentials: new RestCredentials() {Login = credentials.Login, Password = credentials.Password},
                request: new SyncItemsMetaContainerRequest
                {
                    ClientRegistrationId = this.interviewerSettings.GetClientRegistrationId().Value,
                    LastSyncedUserPackageId = lastKnownPackageId,
                    LastSyncedQuestionnairePackageId = lastKnownPackageId,
                    LastSyncedInterviewPackageId = lastKnownPackageId,
                });

            if (syncItemsMetaContainer.UserChunksMeta == null 
                || syncItemsMetaContainer.InterviewChunksMeta == null 
                || syncItemsMetaContainer.QuestionnaireChunksMeta == null)
                throw new Exception(Resources.ErrorOnItemListReceiving);

            return syncItemsMetaContainer;
        }

        public async Task<UserSyncPackageDto> RequestUserPackageAsync(SyncCredentials credentials, string chunkId)
        {
            return await this.DownloadPackage<UserSyncPackageDto>(credentials, chunkId);
        }

        public async Task<QuestionnaireSyncPackageDto> RequestQuestionnairePackageAsync(SyncCredentials credentials, string chunkId)
        {
            return await this.DownloadPackage<QuestionnaireSyncPackageDto>(credentials, chunkId);
        }

        public async Task<InterviewSyncPackageDto> RequestInterviewPackageAsync(SyncCredentials credentials, string chunkId)
        {
            return await this.DownloadPackage<InterviewSyncPackageDto>(credentials, chunkId);
        }

        private async Task<T> DownloadPackage<T>(SyncCredentials credentials, string chunkId)
        {
            var package = await this.restService.PostAsync<T>(
                        url: string.Format("api/InterviewerSync/Get{0}", typeof(T).Name),
                        credentials: new RestCredentials { Login = credentials.Login, Password = credentials.Password },
                        request:
                            new SyncPackageRequest
                            {
                                PackageId = chunkId,
                                ClientRegistrationId = this.interviewerSettings.GetClientRegistrationId().Value
                            });

            if (package == null)
            {
                throw new Exception(Resources.GetSyncPackageExceptionMessage);
            }

            return package;
        }

        public async Task PushChunkAsync(SyncCredentials credentials, string synchronizationPackage)
        {
            if (synchronizationPackage == null) throw new ArgumentNullException("synchronizationPackage");

            try
            {
                await this.restService.PostAsync(
                    url: "api/InterviewerSync/PostPackage",
                    credentials: new RestCredentials() {Login = credentials.Login, Password = credentials.Password},
                    request: new PostPackageRequest() {SynchronizationPackage = synchronizationPackage});
            }
            catch (Exception ex)
            {
                throw new Exception(Resources.PushFailed, ex);
            }
        }

        public async Task PushBinaryAsync(SyncCredentials credentials, Guid interviewId, string fileName, byte[] fileData)
        {
            if (fileData == null) throw new ArgumentNullException("fileData");
            try
            {
                await this.restService.PostAsync(
                    url: "api/InterviewerSync/PostFile",
                    credentials: new RestCredentials() {Login = credentials.Login, Password = credentials.Password},
                    request: new PostFileRequest()
                    {
                        InterviewId = interviewId,
                        FileName = fileName, 
                        Data = Convert.ToBase64String(fileData)
                    });
            }
            catch(Exception ex)
            {
                throw new Exception(Resources.PushBinaryDataFailed, ex);
            }
        }

        public async Task<string> GetChunkIdByTimestampAsync(SyncCredentials credentials, long timestamp)
        {
            return await this.restService.GetAsync<string>(
                url: "api/InterviewerSync/GetPackageIdByTimeStamp",
                credentials: new RestCredentials() {Login = credentials.Login, Password = credentials.Password},
                queryString: new { timestamp });
        }

        public async Task<bool> NewVersionAvailableAsync()
        {
            return await this.restService.GetAsync<bool>(
                url: "api/InterviewerSync/CheckNewVersion",
                queryString: new {versionCode = this.interviewerSettings.GetApplicationVersionCode()});
        }
    }
}
