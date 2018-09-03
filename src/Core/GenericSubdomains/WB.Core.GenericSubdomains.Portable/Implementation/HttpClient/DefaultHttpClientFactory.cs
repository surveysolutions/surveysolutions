using System.Net.Http;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    public class DefaultHttpClientFactory : IHttpClientFactory
    {
        public virtual HttpMessageHandler CreateMessageHandler()
        {
            return new HttpClientHandler();
        }
    }
}
