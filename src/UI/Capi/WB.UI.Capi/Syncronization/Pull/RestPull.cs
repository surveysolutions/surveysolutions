using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Utils.Rest;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.UI.Capi.Syncronization.Pull
{
    public class RestPull
    {
        private readonly IRestServiceWrapper webExecutor;

        private const string GetChunckPath = "api/InterviewerSync/GetSyncPackage";
        private const string GetARKeysPath = "api/InterviewerSync/GetARKeys";

        public RestPull(IRestServiceWrapper webExecutor)
        {
            this.webExecutor = webExecutor;
        }

        public async Task<SyncItem> RequestChunckAsync(string login, string password, string id, string deviceId,
            CancellationToken ct)
        {
            var package = await webExecutor.ExecuteRestRequestAsync<SyncPackage>(GetChunckPath, ct, null,
                login, password, "GET",
                new KeyValuePair<string, object>("packageId", id),
                new KeyValuePair<string, object>("clientRegistrationId", deviceId));

            if (package.ItemsContainer == null || package.ItemsContainer.Count == 0)
                throw new RestException("Content is absent.");

            return package.ItemsContainer[0];
        }

        public async Task<List<SynchronizationChunkMeta>> GetChuncksAsync(string login, string password, string deviceId,
            Guid? lastReceivedPackageId, CancellationToken ct)
        {
            var syncItemsMetaContainer = await webExecutor.ExecuteRestRequestAsync<SyncItemsMetaContainer>(
                GetARKeysPath, ct, null, login, password, "GET",
                new KeyValuePair<string, object>("clientRegistrationId", deviceId),
                new KeyValuePair<string, object>("lastSyncedPackageId",
                    lastReceivedPackageId.HasValue ? (object) lastReceivedPackageId.Value : "")
                );

            if (syncItemsMetaContainer.ChunksMeta == null)
                throw new RestException(Properties.Resource.ErrorOnItemListReceiving);

            return syncItemsMetaContainer.ChunksMeta;
        }

        public async Task<string> GetChunkIdByTimestamp(long timestamp, string login, string password, CancellationToken ct)
        {
            var result = await webExecutor.ExecuteRestRequestAsync<string>(
                "api/InterviewerSync/GetPacakgeIdByTimeStamp",
                ct,
                null,
                login, password, "GET", new KeyValuePair<string, object>("timestamp", timestamp));
            return result;
        }
    }
}
