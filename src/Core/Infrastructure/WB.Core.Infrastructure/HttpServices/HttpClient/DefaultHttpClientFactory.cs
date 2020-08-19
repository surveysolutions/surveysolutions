using System;
using System.Net.Http;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    public class DefaultHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(IHttpStatistician httpStatistician)
        {
            return new HttpClient(new ExtendedMessageHandler(new HttpClientHandler(), httpStatistician));
        }
    }
}
