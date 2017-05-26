using System.Net.Http;
using Flurl.Http.Configuration;

namespace WB.Infrastructure.Shared.Enumerator.Internals
{
    public class ModernHttpClientFactory : DefaultHttpClientFactory
    {
        public override HttpMessageHandler CreateMessageHandler()
        {
            return new ModernHttpClient.NativeMessageHandler();
        }
    }
}