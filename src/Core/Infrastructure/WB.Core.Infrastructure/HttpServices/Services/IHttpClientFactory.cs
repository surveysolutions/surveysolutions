using System.Net.Http;

namespace WB.Core.GenericSubdomains.Portable.Services
{
    public interface IHttpClientFactory
    {
        HttpClient CreateClient(IHttpStatistician httpStatistician);
    }
}
