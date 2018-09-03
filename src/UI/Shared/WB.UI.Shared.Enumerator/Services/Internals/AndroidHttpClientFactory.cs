using System.Net.Http;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.UI.Shared.Enumerator.Services.Internals
{
    public class AndroidHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(Url url, HttpMessageHandler handler, IHttpStatistician statistician)
        {
            return new HttpClient(new ExtendedMessageHandler(handler, statistician));
        }

        public HttpMessageHandler CreateMessageHandler()
        {
            return new Xamarin.Android.Net.AndroidClientHandler();
        }
    }
}
