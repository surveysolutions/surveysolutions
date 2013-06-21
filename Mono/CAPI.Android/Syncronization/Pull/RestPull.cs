using System;
using System.Collections.Generic;
using System.Linq;
using CAPI.Android.Syncronization.RestUtils;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace CAPI.Android.Syncronization.Pull
{
    public class RestPull
    {
        private readonly IRestUrils webExecutor;

        private const string getChunckPath = "sync/GetSyncPackage";
        private const string getARKeysPath = "sync/GetARKeys";

        public RestPull(IRestUrils webExecutor)
        {
            this.webExecutor = webExecutor;
        }

        public SyncItem RequestChunck(string login, string password, Guid id, long sequence, string deviceId)
        {
            var package = webExecutor.ExcecuteRestRequest<SyncPackage>(getChunckPath,
                                                                       new KeyValuePair<string, string>("login", login),
                                                                       new KeyValuePair<string, string>("password", password),
                                                                       new KeyValuePair<string, string>("aRKey", id.ToString()),
                                                                       new KeyValuePair<string, string>("aRSequence", sequence.ToString()),
                                                                       new KeyValuePair<string, string>("clientRegistrationId", deviceId));

            if (package.IsErrorOccured || package.ItemsContainer == null || package.ItemsContainer.Count == 0)
                throw new SynchronizationException("Content is absent.");
            return package.ItemsContainer[0];
        }

        public IDictionary<KeyValuePair<long, Guid>, bool> GetChuncks(string login, string password, string deviceId)
        {
            var syncItemsMetaContainer = 
                webExecutor.ExcecuteRestRequest<SyncItemsMetaContainer>(getARKeysPath,
                                                                       new KeyValuePair<string, string>("login", login),
                                                                       new KeyValuePair<string, string>("password", password),
                                                                       new KeyValuePair<string, string>("clientRegistrationId", deviceId));

            if (syncItemsMetaContainer.IsErrorOccured || syncItemsMetaContainer.ARId == null)
                throw new SynchronizationException("Error on item list receiving.");

            return syncItemsMetaContainer.ARId.ToDictionary(s => s, s => false);
        }

    }
}
