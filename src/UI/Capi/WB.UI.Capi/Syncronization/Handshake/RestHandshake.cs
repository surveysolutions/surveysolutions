using System.Collections.Generic;
using WB.Core.GenericSubdomains.Rest;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection;

namespace WB.UI.Capi.Syncronization.Handshake
{
    public class RestHandshake
    {
        private readonly IRestServiceWrapper webExecutor;
        private readonly ISyncProtocolVersionProvider syncProtocolVersionProvider;
        

        private const string handshakePath = "api/InterviewerSync/GetHandshakePackage";

        public RestHandshake(IRestServiceWrapper webExecutor, ISyncProtocolVersionProvider syncProtocolVersionProvider)
        {
            this.webExecutor = webExecutor;
            this.syncProtocolVersionProvider = syncProtocolVersionProvider;
        }

        public string Execute(string login, string password, string androidId, string appID, string registrationKey)
        {
            var package = this.webExecutor.ExecuteRestRequest<HandshakePackage>(handshakePath,
                login, password, "GET",
                new KeyValuePair<string, object>("clientId", appID),
                new KeyValuePair<string, object>("version", syncProtocolVersionProvider.GetProtocolVersion().ToString()),
                new KeyValuePair<string, object>("clientRegistrationId", registrationKey),
                new KeyValuePair<string, object>("androidId", androidId));

            return package.ClientRegistrationKey.ToString();
        }
    }
}