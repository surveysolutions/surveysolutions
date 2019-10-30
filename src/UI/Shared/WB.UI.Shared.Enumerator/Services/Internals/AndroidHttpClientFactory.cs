using System.Net;
using System.Net.Http;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.UI.Shared.Enumerator.Services.Internals
{
    public class AndroidHttpClientFactory : IHttpClientFactory
    {
        private readonly IRestServiceSettings restServiceSettings;

        public AndroidHttpClientFactory(IRestServiceSettings restServiceSettings)
        {
            this.restServiceSettings = restServiceSettings;
        }

        public HttpClient CreateClient(IHttpStatistician statistician = null)
        {
            var http = new HttpClient(new ExtendedMessageHandler(CreateMessageHandler(), statistician))
            {
                Timeout = this.restServiceSettings.Timeout,
                MaxResponseContentBufferSize = this.restServiceSettings.BufferSize
            };

            http.DefaultRequestHeaders.ConnectionClose = true;
            return http;
        }

        public HttpMessageHandler CreateMessageHandler()
        {
            var messageHandler = new ModernHttpClient.NativeMessageHandler
            {
                Timeout = restServiceSettings.Timeout,
                DisableCaching = true,
                AutomaticDecompression = DecompressionMethods.None,
                AllowAutoRedirect = true,
                Proxy = WebRequest.GetSystemWebProxy(),
                TLSConfig = { DangerousAcceptAnyServerCertificateValidator = restServiceSettings.AcceptUnsignedSslCertificate }
            };

            return messageHandler;
        }
    }
}
