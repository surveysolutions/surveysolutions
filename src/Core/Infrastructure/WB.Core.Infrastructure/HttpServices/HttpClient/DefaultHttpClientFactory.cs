using System.Net.Http;
using WB.Core.Infrastructure.HttpServices.Services;

namespace WB.Core.Infrastructure.HttpServices.HttpClient
{
    public class DefaultHttpClientFactory : IHttpClientFactory
    {
        public System.Net.Http.HttpClient CreateClient(IHttpStatistician httpStatistician)
        {
            return new System.Net.Http.HttpClient(new ExtendedMessageHandler(new HttpClientHandler(), httpStatistician));
        }
    }
}
