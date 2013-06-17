using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using CAPI.Android.Syncronization.RestUtils;
using Main.Core.Events;
using Newtonsoft.Json;
using RestSharp;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Logging;

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

        public SyncItem RequestChunck(string login, string password, Guid id, string rootType, Guid synckId)
        {
            var package = webExecutor.ExcecuteRestRequest<SyncPackage>(getChunckPath,
                                                                       new KeyValuePair<string, string>("login", login),
                                                                       new KeyValuePair<string, string>("password",password),
                                                                       new KeyValuePair<string, string>("aRKey",id.ToString()));

            if (!package.Status || package.ItemsContainer == null || package.ItemsContainer.Count == 0)
                throw new NullReferenceException("content is absent");
            return package.ItemsContainer[0];
        }

        public IDictionary<SyncItemsMeta,bool> GetChuncks(string login, string password, Guid synckId)
        {
            var syncItemsMetaContainer = webExecutor.ExcecuteRestRequest<SyncItemsMetaContainer>(getARKeysPath, 
                                                                       new KeyValuePair<string, string>("login", login),
                                                                       new KeyValuePair<string, string>("password", password));

            return syncItemsMetaContainer.ARId.ToDictionary(s => s, s => false);
        }

    }
}
