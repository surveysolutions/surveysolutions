using System.Net.Http;

namespace WB.Infrastructure.AspNetCore
{
    public interface IHttpClientConfigurator<T>
    {
        void ConfigureHttpClient(HttpClient hc);
    }
}
