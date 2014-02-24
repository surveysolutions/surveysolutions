using System.Collections.Generic;
using RestSharp;
using WB.Core.GenericSubdomains.Rest;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.UI.Capi.Syncronization.Handshake
{
    public class RestHandshake
    {
        private readonly IRestServiceWrapper webExecutor;

        private const string handshakePath = "sync/Handshake";

        public RestHandshake(IRestServiceWrapper webExecutor)
        {
            this.webExecutor = webExecutor;
        }

        public string Execute(string login, string password, string androidId, string appID, string registrationKey)
        {
            var package = this.webExecutor.ExecuteRestRequest<HandshakePackage>(handshakePath,
                login, password, null,
                new KeyValuePair<string, string>("clientId", appID),
                new KeyValuePair<string, string>("clientRegistrationId", registrationKey),
                new KeyValuePair<string, string>("androidId", androidId));

            if (package.IsErrorOccured)
            {
                throw new SynchronizationException("Error occurred during handshake. Message:" + package.ErrorMessage);
            }

            return package.ClientRegistrationKey.ToString();
        }
    }
}