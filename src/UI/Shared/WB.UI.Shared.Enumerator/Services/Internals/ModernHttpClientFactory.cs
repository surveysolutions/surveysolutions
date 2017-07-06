using System.Net.Http;
using Flurl.Http.Configuration;

namespace WB.UI.Shared.Enumerator.Services.Internals
{
    public class ModernHttpClientFactory : DefaultHttpClientFactory
    {
        public override HttpMessageHandler CreateMessageHandler()
        {
            return new ModernHttpClient.NativeMessageHandler();
        }
    }
}