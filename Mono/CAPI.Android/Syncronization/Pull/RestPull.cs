using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using Main.Core.Events;
using Main.Synchronization.Credentials;
using Newtonsoft.Json;
using RestSharp;
using SynchronizationMessages.CompleteQuestionnaire;
using SynchronizationMessages.Synchronization;
using WB.Core.SharedKernel.Utils.Logging;
using WB.Core.Synchronization;

namespace CAPI.Android.Syncronization.Pull
{
    public class RestPull
    {
        private readonly string baseAddress;

        private const string getChunckPath = "importexport/GetSyncPackage";
        private const string getARKeysPath = "importexport/GetARKeys";

        public RestPull(string baseAddress)
        {
            this.baseAddress = baseAddress;
        }

        public byte[] RequestChunck(string login, string password, Guid id, string rootType, Guid synckId)
        {
            var package = ExcecuteRestRequest<SyncPackage>(getChunckPath, login, password,
                                                           new KeyValuePair<string, string>("aRKey", id.ToString()),
                                                           new KeyValuePair<string, string>("rootType", rootType));
            if (!package.Status || package.ItemsContainer == null || package.ItemsContainer.Count == 0)
                throw new NullReferenceException("content is absent");
            return GetBytes(package.ItemsContainer[0].Content);
        }

        public IDictionary<SyncItemsMeta, bool> GetChuncks(string login, string password, Guid synckId)
        {
            var syncItemsMetaContainer = ExcecuteRestRequest<SyncItemsMetaContainer>(getARKeysPath, login, password);

            return syncItemsMetaContainer.ARId.ToDictionary((s) => s, (s) => false);
        }

        protected byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        protected T ExcecuteRestRequest<T>(string url, string login, string password,
                                           params KeyValuePair<string, string>[] additionalParams) where T : class
        {
            var restClient = new RestClient(this.baseAddress);

            var request = new RestRequest(url, Method.POST);
            request.RequestFormat = DataFormat.Json;

            request.AddHeader("Accept-Encoding", "gzip,deflate");
            request.AddParameter("login", login);
            request.AddParameter("password", password);
            foreach (var additionalParam in additionalParams)
            {
                request.AddParameter(additionalParam.Key, additionalParam.Value);
            }
            var response = restClient.Execute(request);

            if (string.IsNullOrWhiteSpace(response.Content) || response.StatusCode != HttpStatusCode.OK)
            {
                var exception = new Exception("Target returned unsupported result.");

                if (response.StatusCode == HttpStatusCode.Forbidden)
                    exception = new AuthenticationException("user wasn't authorized");

                LogManager.GetLogger(GetType())
                          .Error("Sync error. Responce status:" + response.StatusCode, exception);

                throw exception;
            }

            var syncItemsMetaContainer =
                JsonConvert.DeserializeObject<T>(response.Content, new JsonSerializerSettings());

            if (syncItemsMetaContainer == null)
            {
                throw new Exception("Elements to be synchronized are not found.");
            }
            return syncItemsMetaContainer;
        }
    }
}
