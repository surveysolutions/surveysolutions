using System.Net;
using System.Net.Http;
using WB.Core.GenericSubdomains.Portable.Implementation;
using Xamarin.Android.Net;

namespace WB.UI.Shared.Enumerator.Services.Internals
{
    public class AndroidHttpClientFactory : DefaultHttpClientFactory
    {
        public override HttpMessageHandler CreateMessageHandler()
        {
            var handler = new AndroidClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            return handler;
        }
    }
}
