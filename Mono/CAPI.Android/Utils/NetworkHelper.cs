using CAPI.Android.Settings;

namespace CAPI.Android.Utils
{
    using System.Linq;

    using global::Android.Content;
    using global::Android.Net;

    public static class NetworkHelper
    {
        public static bool IsNetworkEnabled(Context context)
        {
            var connectivityMAnager = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);

            return connectivityMAnager.GetAllNetworkInfo().ToList().Select(n => n.IsConnected).Any();
        }
       
    }
}