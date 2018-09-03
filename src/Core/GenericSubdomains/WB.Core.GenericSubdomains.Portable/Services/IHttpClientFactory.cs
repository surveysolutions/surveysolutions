using System.Net.Http;
using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.Core.GenericSubdomains.Portable.Services
{
    public interface IHttpClientFactory
    {
        HttpClient CreateClient(Url url, HttpMessageHandler handler, IHttpStatistician statistician = null);

        HttpMessageHandler CreateMessageHandler();
    }
}
