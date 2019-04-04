using System;
using System.Net;
using System.Net.Http;
using Android.OS;
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
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                var messageHandler = new NativeMessageHandler
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
            else
            {
                var messageHandler = new Xamarin.Android.Net.AndroidClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.None,
                    AllowAutoRedirect = true,
                    Proxy = WebRequest.GetSystemWebProxy()
                };
                if (restServiceSettings.AcceptUnsignedSslCertificate)
                {
                    messageHandler.ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true;
                }

                return messageHandler;
            }
        }
    }
}
