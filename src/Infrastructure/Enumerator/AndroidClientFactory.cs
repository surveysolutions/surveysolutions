using System.Net;
using System.Net.Http;
using Android.App;
using Android.Content;
using Android.Net;
using Flurl.Http.Configuration;
using Xamarin.Android.Net;

namespace WB.Infrastructure.Shared.Enumerator
{
    public class AndroidClientFactory : DefaultHttpClientFactory
    {
        public override HttpMessageHandler CreateMessageHandler()
        {
            var cm = Application.Context.GetSystemService(Context.ConnectivityService) as ConnectivityManager;
            var proxy = cm.DefaultProxy;

            var handler = new AndroidClientHandler();

            if (proxy != null)
                handler.Proxy = new WebProxy(proxy.Host, proxy.Port);

            return handler;
        }
    }
}