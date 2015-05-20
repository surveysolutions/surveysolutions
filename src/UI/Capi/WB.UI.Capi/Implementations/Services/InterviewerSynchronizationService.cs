using System;
using System.Threading.Tasks;

using WB.Core.BoundedContexts.Capi.Implementation.Authorization;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Capi.Properties;

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

        public async Task<SyncItemsMetaContainer> GetPackageIdsToDownloadAsync(SyncCredentials credentials, string type, string lastSyncedPackageId)
        {
            return await this.SyncItemsMetaContainer(credentials, lastSyncedPackageId, type);
        }

        public async Task<HandshakePackage> HandshakeAsync(SyncCredentials credentials, bool shouldThisDeviceBeLinkedToUser = false)
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

            return package;
        }

        private async Task<SyncItemsMetaContainer> SyncItemsMetaContainer(SyncCredentials credentials, string lastSyncedUserPackageId, string type)
        {
            var request = new SyncItemsMetaContainerRequest
                          {
                              ClientRegistrationId = this.interviewerSettings.GetClientRegistrationId().Value,
                              LastSyncedPackageId = lastSyncedUserPackageId,
                          };

            var syncItemsMetaContainer = await this.restService.PostAsync<SyncItemsMetaContainer>(
                        url: string.Format("api/InterviewerSync/Get{0}PackageIds", type),
                        credentials: new RestCredentials { Login = credentials.Login, Password = credentials.Password },
                        request: request);

            return syncItemsMetaContainer;
        }

        public async Task<UserSyncPackageDto> RequestUserPackageAsync(SyncCredentials credentials, string chunkId)
        {
            return await this.DownloadPackage<UserSyncPackageDto>(credentials, chunkId, "GetUserSyncPackage");
        }

        public async Task<QuestionnaireSyncPackageDto> RequestQuestionnairePackageAsync(SyncCredentials credentials, string chunkId)
        {
            return await this.DownloadPackage<QuestionnaireSyncPackageDto>(credentials, chunkId, "GetQuestionnaireSyncPackage");
        }

        public async Task<InterviewSyncPackageDto> RequestInterviewPackageAsync(SyncCredentials credentials, string chunkId)
        {
            return await this.DownloadPackage<InterviewSyncPackageDto>(credentials, chunkId, "GetInterviewSyncPackage");
        }

        private async Task<T> DownloadPackage<T>(SyncCredentials credentials, string chunkId, string Url)
        {
            var package = await this.restService.PostAsync<T>(
                        url: string.Format("api/InterviewerSync/{0}", Url),
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

        public async Task PushChunkAsync(SyncCredentials credentials, string synchronizationPackage, Guid interviewId)
        {
            if (synchronizationPackage == null) throw new ArgumentNullException("synchronizationPackage");

            try
            {
                await this.restService.PostAsync(
                    url: "api/InterviewerSync/PostPackage",
                    credentials: new RestCredentials() { Login = credentials.Login, Password = credentials.Password },
                    request: new PostPackageRequest() { InterviewId = interviewId, SynchronizationPackage = synchronizationPackage });
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

        public async Task<bool> NewVersionAvailableAsync()
        {
            return await this.restService.GetAsync<bool>(
                url: "api/InterviewerSync/CheckNewVersion",
                queryString: new {versionCode = this.interviewerSettings.GetApplicationVersionCode()});
        }
    }
}
