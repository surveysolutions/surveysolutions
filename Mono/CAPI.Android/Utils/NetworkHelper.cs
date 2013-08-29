namespace CAPI.Android.Utils
{
    using System.Linq;

    using global::Android.Content;
    using global::Android.Net;

    public static class NetworkHelper
    {
        public static bool IsNetworkEnabled(Context context)
        {
            var cm = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
            
            return cm.GetAllNetworkInfo().Where(networkInfo => networkInfo.Type == ConnectivityType.Wifi || networkInfo.Type == ConnectivityType.Mobile)
                     .Any(networkInfo => networkInfo.IsConnectedOrConnecting);
        }

    }
}