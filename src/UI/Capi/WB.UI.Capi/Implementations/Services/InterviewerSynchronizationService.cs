using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CAPI.Android.Core.Model.Authorization;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Capi.Properties;
using WB.UI.Capi.Services;
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

        public async Task<Guid> HandshakeAsync(SyncCredentials credentials)
        {
            var package = await this.restService.PostAsync<HandshakePackage>(
                url: "api/InterviewerSync/GetHandshakePackage",
                credentials: new RestCredentials() {Login = credentials.Login, Password = credentials.Password},
                request: new HandshakePackageRequest()
                {
                    ClientId = this.interviewerSettings.GetInstallationId(),
                    Version = this.syncProtocolVersionProvider.GetProtocolVersion(),
                    ClientRegistrationId = this.interviewerSettings.GetClientRegistrationId(),
                    AndroidId = this.interviewerSettings.GetDeviceId()
                });

            return package.ClientRegistrationKey;
        }

        public async Task<IEnumerable<SynchronizationChunkMeta>> GetChunksAsync(SyncCredentials credentials, string lastKnownPackageId)
        {
            var syncItemsMetaContainer = await restService.PostAsync<SyncItemsMetaContainer>(
                url: "api/InterviewerSync/GetARKeys",
                credentials: new RestCredentials() {Login = credentials.Login, Password = credentials.Password},
                request: new SyncItemsMetaContainerRequest()
                {
                    ClientRegistrationId = this.interviewerSettings.GetClientRegistrationId().Value,
                    LastSyncedPackageId = lastKnownPackageId
                });

            if (syncItemsMetaContainer.ChunksMeta == null)
                throw new Exception(Resources.ErrorOnItemListReceiving);

            return syncItemsMetaContainer.ChunksMeta;
        }

        public async Task<SyncItem> RequestChunkAsync(SyncCredentials credentials, string chunkId)
        {
            var package = await restService.PostAsync<SyncPackage>(
                url: "api/InterviewerSync/GetSyncPackage",
                credentials: new RestCredentials() {Login = credentials.Login, Password = credentials.Password},
                request: new SyncPackageRequest()
                    {
                        PackageId = chunkId,
                        ClientRegistrationId = this.interviewerSettings.GetClientRegistrationId().Value
                    });

            if (package == null || package.SyncItem == null)
                throw new Exception(Resources.GetSyncPackageExceptionMessage);

            return package.SyncItem;
        }

        public async Task PushChunkAsync(SyncCredentials credentials, Guid interviewId, string synchronizationPackage)
        {
            if (synchronizationPackage == null) throw new ArgumentNullException("synchronizationPackage");

            try
            {
                await this.restService.PostAsync(
                    url: "api/InterviewerSync/PostPackage",
                    credentials: new RestCredentials() {Login = credentials.Login, Password = credentials.Password},
                    request: new PostPackageRequest() {SynchronizationPackage = synchronizationPackage, InterviewId = interviewId});
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
