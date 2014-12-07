using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CAPI.Android.Core.Model.Authorization;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Implementation.Services.Rest;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.GenericSubdomains.Utils.Services.Rest;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.UI.Capi.Services;
using WB.UI.Capi.Settings;

namespace WB.UI.Capi.Implementations.Services
{
    internal class SynchronizationService : ISynchronizationService
    {
        private readonly IRestService restService;
        private readonly IInterviewerSettings interviewerSettings;

        public SynchronizationService(IRestService restService, IInterviewerSettings interviewerSettings)
        {
            this.restService = restService;
            this.interviewerSettings = interviewerSettings;
        }

        public async Task<string> HandshakeAsync(SyncCredentials credentials, CancellationToken token = default(CancellationToken))
        {
            var package = await this.restService.GetAsync<HandshakePackage>(
                url: "api/InterviewerSync/GetHandshakePackage",
                credentials: new RestCredentials() {Login = credentials.Login, Password = credentials.Password},
                token: token,
                requestData:
                    new
                    {
                        clientId = this.interviewerSettings.GetInstallationId(),
                        version = this.interviewerSettings.GetApplicationVersionCode(),
                        clientRegistrationId = this.interviewerSettings.GetClientRegistrationId(),
                        androidId = this.interviewerSettings.GetDeviceId()
                    });

            return package.ClientRegistrationKey.ToString();
        }

        public async Task<List<SynchronizationChunkMeta>> GetChunksAsync(SyncCredentials credentials, CancellationToken token)
        {
            var syncItemsMetaContainer = await restService.GetAsync<SyncItemsMetaContainer>(
                url: "api/InterviewerSync/GetARKeys",
                credentials: new RestCredentials() {Login = credentials.Login, Password = credentials.Password},
                token: token,
                requestData:
                    new
                    {
                        clientRegistrationId = this.interviewerSettings.GetClientRegistrationId(),
                        lastSyncedPackageId = this.interviewerSettings.GetLastReceivedPackageId()
                    });

            if (syncItemsMetaContainer.ChunksMeta == null)
                throw new RestException(Properties.Resource.ErrorOnItemListReceiving);

            return syncItemsMetaContainer.ChunksMeta;
        }

        public async Task<SyncItem> RequestChunkAsync(SyncCredentials credentials, Guid chunkId, CancellationToken token)
        {
            var package = await restService.GetAsync<SyncPackage>(
                url: "api/InterviewerSync/GetSyncPackage",
                credentials: new RestCredentials() {Login = credentials.Login, Password = credentials.Password},
                token: token,
                requestData:
                    new
                    {
                        aRKey = chunkId,
                        clientRegistrationId = this.interviewerSettings.GetClientRegistrationId()

                    });

            if (package.ItemsContainer == null || package.ItemsContainer.Count == 0)
                throw new RestException("Content is absent.");

            return package.ItemsContainer[0];
        }

        public async Task PushChunkAsync(SyncCredentials credentials, string synchronizationPackage, CancellationToken token)
        {
            if (synchronizationPackage == null) throw new ArgumentNullException("synchronizationPackage");

            try
            {
                await this.restService.PostAsync(
                    url: "api/InterviewerSync/PostPackage",
                    credentials: new RestCredentials() {Login = credentials.Login, Password = credentials.Password},
                    requestData: synchronizationPackage,
                    token: token);
            }
            catch
            {
                throw new RestException(Properties.Resource.PushFailed);
            }
        }

        public async Task PushBinaryAsync(SyncCredentials credentials, Guid interviewId, string fileName, byte[] fileData, CancellationToken token)
        {
            if (fileData == null) throw new ArgumentNullException("fileData");
            try
            {
                await this.restService.PostAsync(
                    url: "api/InterviewerSync/PostFile", token: token,
                    credentials: new RestCredentials() {Login = credentials.Login, Password = credentials.Password},
                    requestData:
                        new
                        {
                            interviewId = interviewId,
                            fileName = fileName,
                            base64BinaryData = Convert.ToBase64String(fileData)
                        });
            }
            catch
            {
                throw new RestException(Properties.Resource.PushBinaryDataFailed);
            }
        }

        public async Task<Guid> GetChunkIdByTimestamp(SyncCredentials credentials, long timestamp, CancellationToken token)
        {
            return await this.restService.GetAsync<Guid>(
                url: "api/InterviewerSync/GetPacakgeIdByTimeStamp",
                credentials: new RestCredentials() {Login = credentials.Login, Password = credentials.Password},
                token: token,
                requestData: new {timestamp = timestamp});
        }

        public async Task<bool> NewVersionAvailableAsync(CancellationToken token = default(CancellationToken))
        {
            return await this.restService.GetAsync<bool>(
                url: "api/InterviewerSync/CheckNewVersion",
                requestData: new {versionCode = this.interviewerSettings.GetApplicationVersionCode()},
                token: token);
        }
    }
}
