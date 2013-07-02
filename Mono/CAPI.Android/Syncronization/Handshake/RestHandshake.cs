using System.Collections.Generic;
using CAPI.Android.Syncronization.RestUtils;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace CAPI.Android.Syncronization.Handshake
{
    public class RestHandshake
    {
        private readonly IRestUrils webExecutor;

        private const string handshakePath = "sync/Handshake";

        public RestHandshake(IRestUrils webExecutor)
        {
            this.webExecutor = webExecutor;
        }

        public string Execute(string login, string password, string androidId, string appID, string registrationKey)
        {
            var package = webExecutor.ExcecuteRestRequest<HandshakePackage>(handshakePath,
                                                                       new KeyValuePair<string, string>("login", login),
                                                                       new KeyValuePair<string, string>("password", password),
                                                                       new KeyValuePair<string, string>("clientId", appID),
                                                                       new KeyValuePair<string, string>("clientRegistrationId", registrationKey),
                                                                       new KeyValuePair<string, string>("androidId", androidId));

            if (package.IsErrorOccured)
            {
                throw new SynchronizationException("Error occured during handshake. Message:" + package.ErrorMessage);
            }

            return package.ClientRegistrationKey.ToString();
        }
    }
}