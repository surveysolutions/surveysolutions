using System.Net;
using System.Security.Authentication;
using Main.Core.Utility;
using Main.Synchronization.Credentials;
using RestSharp;

namespace CAPI.Android.Syncronization
{
    public class RestAuthenticator : ISyncAuthenticator
    {
        /// <summary>
        /// The item path.
        /// </summary>
        private const string ValidateUserPath = "importexport/Validate";

        public RestAuthenticator(string baseAddress)
        {
            this.baseAddress = baseAddress;
        }

        /// <summary>
        /// The base address.
        /// </summary>
        private readonly string baseAddress;

        public event RequestCredentialsCallBack RequestCredentials;

        protected SyncCredentials? OnRequestCredentials()
        {
            var handler = RequestCredentials;
            if (handler == null)
                return null;
            return handler(this);
        }

        public bool ValidateUser()
        {
            var credentials = OnRequestCredentials();
            if (!credentials.HasValue)
                return false;

            var restClient = new RestClient(this.baseAddress);

            var request = new RestRequest(ValidateUserPath, Method.POST);
            request.AddParameter("login", credentials.Value.Login);
            request.AddParameter("password", credentials.Value.Password);
            request.RequestFormat = DataFormat.Json;
            IRestResponse response = restClient.Execute(request);

            if (string.IsNullOrWhiteSpace(response.Content) || response.StatusCode != HttpStatusCode.OK)
            {
                return false;
            }
            bool result;
            if (!bool.TryParse(response.Content, out result))
                return false;
            return result;
        }

    }
}
