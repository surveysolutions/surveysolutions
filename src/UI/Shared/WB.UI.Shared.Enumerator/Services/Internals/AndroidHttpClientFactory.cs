using System.Net;
using System.Net.Http;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.Infrastructure.HttpServices.Services;
using Xamarin.Android.Net;

namespace WB.UI.Shared.Enumerator.Services.Internals
{
    public class AndroidHttpClientFactory : IHttpClientFactory
    {
        private readonly IRestServiceSettings restServiceSettings;

        public AndroidHttpClientFactory(IRestServiceSettings restServiceSettings)
        {
            this.restServiceSettings = restServiceSettings;
        }

        private HttpClient httpClient;

        public HttpClient CreateClient(IHttpStatistician statistician = null)
        {
            if (httpClient != null) return httpClient;

            var http = new HttpClient(new ExtendedMessageHandler(CreateMessageHandler(), statistician))
            {
                Timeout = this.restServiceSettings.Timeout,
                MaxResponseContentBufferSize = this.restServiceSettings.BufferSize
            };

            http.DefaultRequestHeaders.ConnectionClose = true;
            httpClient = http;
            return http;
        }

        private HttpMessageHandler CreateMessageHandler()
        {
            var messageHandler = new AndroidMessageHandler
            {
                ConnectTimeout = restServiceSettings.Timeout,
                AutomaticDecompression = DecompressionMethods.None,
                AllowAutoRedirect = true
            };
            
            if (this.restServiceSettings.AcceptUnsignedSslCertificate)
            {
                messageHandler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            return messageHandler;
        }
    }
}
