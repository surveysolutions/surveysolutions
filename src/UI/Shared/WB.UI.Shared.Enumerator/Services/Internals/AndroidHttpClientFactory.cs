using System.Net.Http;
using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.UI.Shared.Enumerator.Services.Internals
{
    public class AndroidHttpClientFactory : DefaultHttpClientFactory
    {
        public override HttpMessageHandler CreateMessageHandler()
        {
            return new Xamarin.Android.Net.AndroidClientHandler();
        }
    }
}
