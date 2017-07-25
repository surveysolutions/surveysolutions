using System.Net.Http;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    public class DefaultHttpClientFactory : IHttpClientFactory
    {
        public virtual HttpClient CreateClient(Url url, HttpMessageHandler handler, IHttpStatistician statistician)
        {
            return new HttpClient(new ExtendedMessageHandler(handler, statistician));
        }

        public virtual HttpMessageHandler CreateMessageHandler()
        {
            return new HttpClientHandler();
        }
    }

    public interface IHttpClientFactory
    {
        HttpClient CreateClient(Url url, HttpMessageHandler handler, IHttpStatistician statistician = null);

        HttpMessageHandler CreateMessageHandler();
    }
}