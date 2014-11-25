using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Rest;
using WB.Core.GenericSubdomains.Utils.Rest;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.UI.Capi.Syncronization.Pull
{
    public class RestPull
    {
        private readonly IRestServiceWrapper webExecutor;

        private const string getChunckPath = "api/InterviewerSync/GetSyncPackage";
        private const string getARKeysPath = "api/InterviewerSync/GetARKeys";

        public RestPull(IRestServiceWrapper webExecutor)
        {
            this.webExecutor = webExecutor;
        }

        public async Task<SyncItem> RequestChunckAsync(string login, string password, Guid id, string deviceId,
            CancellationToken ct)
        {
            var package = await webExecutor.ExecuteRestRequestAsync<SyncPackage>(getChunckPath, ct, null,
                login, password, "GET",
                new KeyValuePair<string, object>("aRKey", id),
                new KeyValuePair<string, object>("clientRegistrationId", deviceId));

            if (package.ItemsContainer == null || package.ItemsContainer.Count == 0)
                throw new RestException("Content is absent.");
            
            return package.ItemsContainer[0];
        }

        public async Task<Dictionary<SynchronizationChunkMeta, bool>> GetChuncksAsync(string login, string password, string deviceId,
            string sequence, CancellationToken ct)
        {
            var syncItemsMetaContainer = await webExecutor.ExecuteRestRequestAsync<SyncItemsMetaContainer>(
                getARKeysPath, ct, null, login, password, "GET",
                new KeyValuePair<string, object>("clientRegistrationId", deviceId),
                new KeyValuePair<string, object>("lastSyncedPackageId", sequence)
                );

            if (syncItemsMetaContainer.ChunksMeta == null)
                throw new RestException(Properties.Resource.ErrorOnItemListReceiving);

            return syncItemsMetaContainer.ChunksMeta.ToDictionary(s => s, s => false);
        }
    }
}
