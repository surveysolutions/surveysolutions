using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using Main.Synchronization.Credentials;
using RestSharp;
using SynchronizationMessages.Synchronization;
using WB.Core.SharedKernel.Utils.Logging;

namespace CAPI.Android.Syncronization.Pull
{
    public class RestPull
    {
        private readonly string baseAddress;
        private const string getChunckPath = "GetChunckPath";

        public RestPull(string baseAddress)
        {
            this.baseAddress = baseAddress;
        }
        public byte[] RequestChunck(Guid id,Guid synckId)
        {
            Thread.Sleep(500);
            return PackageHelper.Compress(string.Format("hello capi pull {0}", id));
            /*var restClient = new RestClient(this.baseAddress);
            var request = new RestRequest(getChunckPath, Method.POST);
            var currentCredentials = validator.RequestCredentials();
            request.AddParameter("login", currentCredentials.Login);
            request.AddParameter("password", currentCredentials.Password);
            request.AddParameter("chunckId", id);

            IRestResponse response = restClient.Execute(request);

            if (string.IsNullOrWhiteSpace(response.Content) || response.StatusCode != HttpStatusCode.OK)
            {
                var exception = new Exception("Target returned unsupported result.");

                if (response.StatusCode == HttpStatusCode.Forbidden)
                    exception = new AuthenticationException("user wasn't authorized");

                LogManager.GetLogger(GetType())
                    .Error("Sync error. Responce status:" + response.StatusCode, exception);

                throw exception;
            }
            return response.Content;*/
        }

        public IDictionary<Guid, bool> GetChuncks(Guid synckId)
        {
            Thread.Sleep(1000);
            var retval = new Dictionary<Guid, bool>();
            for (int i = 0; i < 3; i++)
            {
                retval.Add(Guid.NewGuid(), false);
            }

            return retval;
        }
    }
}
