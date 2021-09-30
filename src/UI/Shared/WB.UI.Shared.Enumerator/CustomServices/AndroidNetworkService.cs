using Android.App;
using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Android.Telephony;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    public class AndroidNetworkService : INetworkService
    {
        private readonly string unknown = "UNKNOWN";

       public AndroidNetworkService()
       {
       }

        public bool IsNetworkEnabled()
        {
            var cm = (ConnectivityManager)Application.Context.GetSystemService(Context.ConnectivityService);
            return cm.ActiveNetworkInfo?.IsConnectedOrConnecting ?? false;
        }

        private NetworkInfo GetNetworkInfo()
        {
            ConnectivityManager cm = (ConnectivityManager)Application.Context.GetSystemService(Context.ConnectivityService);
            return cm.ActiveNetworkInfo;
        }

        public string GetNetworkType()
        {
            var network = this.GetNetworkInfo();
            if (network == null)
                return unknown;

            return this.GetNetworkInfo().TypeName;
        }

        public string GetNetworkName()
        {
            var network = this.GetNetworkInfo();

            if (network == null)
                return unknown;

            if (network.Type == ConnectivityType.Wifi)
            {
                WifiManager manager = (WifiManager)Application.Context.GetSystemService(Context.WifiService);
                return manager.ConnectionInfo.SSID;
            }
            if (network.Type == ConnectivityType.Mobile)
            {
                TelephonyManager manager = (TelephonyManager)Application.Context.GetSystemService(Context.TelephonyService);
                return manager.NetworkOperatorName;
            }
            return this.GetNetworkInfo().Type.ToString();
        }
    }
}
