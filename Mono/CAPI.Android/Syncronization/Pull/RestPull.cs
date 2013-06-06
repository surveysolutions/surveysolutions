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
        private readonly ISyncAuthenticator validator;

        public RestPull(string baseAddress, ISyncAuthenticator validator)
        {
            this.baseAddress = baseAddress;
            this.validator = validator;
        }
        public byte[] RequestChunck(Guid id)
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
    }
}
