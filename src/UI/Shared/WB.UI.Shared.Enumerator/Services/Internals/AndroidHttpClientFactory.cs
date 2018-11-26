using System;
using System.Net;
using System.Net.Http;
using ModernHttpClient;
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

        public HttpClient CreateClient(Url url, HttpMessageHandler handler, IHttpStatistician statistician)
        {
            var httpClient = new HttpClient(new ExtendedMessageHandler(handler, statistician));
            httpClient.DefaultRequestHeaders.ConnectionClose = true;
            //httpClient.DefaultRequestHeaders.Add("Connection", "close");
            return httpClient;
        }

        public HttpMessageHandler CreateMessageHandler()
        {
            var messageHandler = new NativeMessageHandler()
            {
                Timeout = restServiceSettings.Timeout,
                EnableUntrustedCertificates = restServiceSettings.AcceptUnsignedSslCertificate,
                DisableCaching = true,
                AutomaticDecompression = DecompressionMethods.None,
                AllowAutoRedirect = true,
                Proxy = WebRequest.GetSystemWebProxy()
            };

            return messageHandler;
        }
    }
}
