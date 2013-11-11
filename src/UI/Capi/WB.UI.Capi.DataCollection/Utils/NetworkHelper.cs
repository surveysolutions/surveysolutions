using System.Linq;
using Android.Content;
using Android.Net;

namespace WB.UI.Capi.DataCollection.Utils
{
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