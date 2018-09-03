using System.Net.Http;
using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.Core.GenericSubdomains.Portable.Services
{
    public interface IHttpClientFactory
    {
        HttpMessageHandler CreateMessageHandler();
    }
}
