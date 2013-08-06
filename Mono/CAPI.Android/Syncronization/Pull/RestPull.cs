using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CAPI.Android.Syncronization.RestUtils;
using RestSharp;
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

        public SyncItem RequestChunck(string login, string password, Guid id, long sequence, string deviceId, CancellationToken ct)
        {
            try
            {
                var package = webExecutor.ExcecuteRestRequestAsync<SyncPackage>(getChunckPath, ct, null,
                    new HttpBasicAuthenticator(login, password),
                     new KeyValuePair<string, string>("aRKey", id.ToString()),
                     new KeyValuePair<string, string>("aRSequence", sequence.ToString()),
                     new KeyValuePair<string, string>("clientRegistrationId", deviceId));

                if (package.IsErrorOccured || package.ItemsContainer == null || package.ItemsContainer.Count == 0)
                    throw new SynchronizationException("Content is absent.");
                return package.ItemsContainer[0];
            }
            catch (RestException)
            {
                throw new SynchronizationException("Data reciving was canceled.");
            }
        }

        public IDictionary<KeyValuePair<long, Guid>, bool> GetChuncks(string login, string password, string deviceId, string sequence, CancellationToken ct)
        {
            try
            {
                var syncItemsMetaContainer = webExecutor.ExcecuteRestRequestAsync<SyncItemsMetaContainer>(
                                                                       getARKeysPath, ct, null,
                                                                       new HttpBasicAuthenticator(login, password),
                                                                       new KeyValuePair<string, string>("clientRegistrationId", deviceId),
                                                                       new KeyValuePair<string, string>("sequence", sequence)
                                                                       );

                if (syncItemsMetaContainer.IsErrorOccured || syncItemsMetaContainer.ARId == null)
                    throw new SynchronizationException("Error on item list receiving.");

                return syncItemsMetaContainer.ARId.ToDictionary(s => s, s => false);
            }
            catch (RestException)
            {
                throw new SynchronizationException("Data receiving was canceled.");
            }
        }

    }
}
