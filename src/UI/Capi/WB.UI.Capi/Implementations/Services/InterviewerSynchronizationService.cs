using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CAPI.Android.Core.Model.Authorization;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernel.Utils.Implementation.Services;
using WB.Core.SharedKernel.Utils.Services.Rest;
using WB.UI.Capi.Services;
using WB.UI.Capi.Settings;

namespace WB.UI.Capi.Implementations.Services
{
    internal class InterviewerSynchronizationService : ISynchronizationService
    {
        private readonly IRestService restService;
        private readonly IInterviewerSettings interviewerSettings;
        private readonly ILocalizationService localizationService;

        public InterviewerSynchronizationService(IRestService restService, IInterviewerSettings interviewerSettings, ILocalizationService localizationService)
        {
            this.restService = restService;
            this.interviewerSettings = interviewerSettings;
            this.localizationService = localizationService;
        }

        public async Task<Guid> HandshakeAsync(SyncCredentials credentials, CancellationToken token = default(CancellationToken))
        {
            var package = await this.restService.PostAsync<HandshakePackage>(
                url: "api/InterviewerSync/GetHandshakePackage",
                credentials: new RestCredentials() {Login = credentials.Login, Password = credentials.Password},
                token: token,
                requestQueryString: new HandshakePackageRequest()
                {
                    ClientId = this.interviewerSettings.GetInstallationId(),
                    Version = this.interviewerSettings.GetApplicationVersionCode(),
                    ClientRegistrationId = this.interviewerSettings.GetClientRegistrationId(),
                    AndroidId = this.interviewerSettings.GetDeviceId()
                });

            return package.ClientRegistrationKey;
        }

        public async Task<IEnumerable<SynchronizationChunkMeta>> GetChunksAsync(SyncCredentials credentials, CancellationToken token, Guid? lastKnownPackageId)
        {
            var syncItemsMetaContainer = await restService.PostAsync<SyncItemsMetaContainer>(
                url: "api/InterviewerSync/GetARKeys",
                credentials: new RestCredentials() {Login = credentials.Login, Password = credentials.Password},
                token: token,
                requestQueryString: new SyncItemsMetaContainerRequest()
                {
                    ClientRegistrationId = this.interviewerSettings.GetClientRegistrationId().Value,
                    LastSyncedPackageId = lastKnownPackageId
                });

            if (syncItemsMetaContainer.ChunksMeta == null)
                throw new RestException(this.localizationService.GetString("ErrorOnItemListReceiving"));

            return syncItemsMetaContainer.ChunksMeta;
        }

        public async Task<SyncItem> RequestChunkAsync(SyncCredentials credentials, Guid chunkId, CancellationToken token)
        {
            var package = await restService.PostAsync<SyncPackage>(
                url: "api/InterviewerSync/GetSyncPackage",
                credentials: new RestCredentials() {Login = credentials.Login, Password = credentials.Password},
                token: token,
                requestQueryString: new SyncPackageRequest()
                    {
                        ARKey = chunkId,
                        ClientRegistrationId = this.interviewerSettings.GetClientRegistrationId().Value
                    });

            if (package == null || package.SyncItem == null)
                throw new RestException(this.localizationService.GetString("GetSyncPackageExceptionMessage"));

            return package.SyncItem;
        }

        public async Task PushChunkAsync(SyncCredentials credentials, string synchronizationPackage, CancellationToken token)
        {
            if (synchronizationPackage == null) throw new ArgumentNullException("synchronizationPackage");

            try
            {
                await this.restService.PostAsync(
                    url: "api/InterviewerSync/PostPackage",
                    credentials: new RestCredentials() {Login = credentials.Login, Password = credentials.Password},
                    requestQueryString: new PostPackageRequest() {SynchronizationPackage = synchronizationPackage},
                    token: token);
            }
            catch (Exception ex)
            {
                throw new RestException(this.localizationService.GetString("PushFailed"));
            }
        }

        public async Task PushBinaryAsync(SyncCredentials credentials, Guid interviewId, string fileName, byte[] fileData, CancellationToken token)
        {
            if (fileData == null) throw new ArgumentNullException("fileData");
            try
            {
                await this.restService.PostAsync(
                    url: "api/InterviewerSync/PostFile", 
                    token: token,
                    credentials: new RestCredentials() {Login = credentials.Login, Password = credentials.Password},
                    requestBody: new PostFileRequest()
                    {
                        InterviewId = interviewId,
                        FileName = fileName,
                        Base64BinaryData = Convert.ToBase64String(fileData)
                    });
            }
            catch
            {
                throw new RestException(this.localizationService.GetString("PushBinaryDataFailed"));
            }
        }

        public Task<Guid> GetChunkIdByTimestamp(SyncCredentials credentials, long timestamp, CancellationToken token)
        {
            return this.restService.GetAsync<Guid>(
                url: "api/InterviewerSync/GetPacakgeIdByTimeStamp",
                credentials: new RestCredentials() {Login = credentials.Login, Password = credentials.Password},
                token: token,
                requestQueryString: timestamp);
        }

        public Task<bool> NewVersionAvailableAsync(CancellationToken token = default(CancellationToken))
        {
            return this.restService.GetAsync<bool>(
                url: "api/InterviewerSync/CheckNewVersion",
                requestQueryString: new {versionCode = this.interviewerSettings.GetApplicationVersionCode()},
                token: token);
        }
    }
}
