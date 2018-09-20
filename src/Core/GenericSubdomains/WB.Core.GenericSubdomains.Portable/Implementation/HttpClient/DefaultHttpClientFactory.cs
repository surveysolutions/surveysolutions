using System.Net.Http;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    public class DefaultHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(Url url, HttpMessageHandler handler, IHttpStatistician statistician)
        {
            return new HttpClient(new ExtendedMessageHandler(handler, statistician));
        }

        public HttpMessageHandler CreateMessageHandler()
        {
            return new HttpClientHandler();
        }
    }
}
