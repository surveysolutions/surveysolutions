using System.Net.Http;
using WB.Core.GenericSubdomains.Portable.Implementation;

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