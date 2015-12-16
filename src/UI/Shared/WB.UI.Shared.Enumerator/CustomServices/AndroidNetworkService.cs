using System.Linq;
using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Android.Telephony;
using Cirrious.CrossCore.Droid.Platform;
using MvvmCross.Plugins.Network.Reachability;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    public class AndroidNetworkService : INetworkService
    {
        private readonly IMvxAndroidCurrentTopActivity mvxAndroidCurrentTopActivity;
        private readonly IMvxReachability mvxReachability;

        public AndroidNetworkService(IMvxAndroidCurrentTopActivity mvxAndroidCurrentTopActivity, IMvxReachability mvxReachability)
        {
            this.mvxAndroidCurrentTopActivity = mvxAndroidCurrentTopActivity;
            this.mvxReachability = mvxReachability;
        }

        public bool IsNetworkEnabled()
        {
            var cm = (ConnectivityManager)this.mvxAndroidCurrentTopActivity.Activity.GetSystemService(Context.ConnectivityService);

            return cm.GetAllNetworkInfo().Where(networkInfo => networkInfo.Type == ConnectivityType.Wifi || networkInfo.Type == ConnectivityType.Mobile)
                     .Any(networkInfo => networkInfo.IsConnectedOrConnecting);
        }

        public bool IsHostReachable(string host)
        {
            return this.mvxReachability.IsHostReachable(host);
        }

        private NetworkInfo GetNetworkInfo()
        {
            ConnectivityManager cm = (ConnectivityManager)this.mvxAndroidCurrentTopActivity.Activity.GetSystemService(Context.ConnectivityService);
            return cm.ActiveNetworkInfo;
        }

        public string GetNetworkTypeName()
        {
            return this.GetNetworkInfo().TypeName;
        }

        public string GetNetworkName()
        {
            var network = this.GetNetworkInfo();
            if (network.Type == ConnectivityType.Wifi)
            {
                WifiManager manager = (WifiManager)this.mvxAndroidCurrentTopActivity.Activity.GetSystemService(Context.WifiService);
                return manager.ConnectionInfo.SSID;
            }
            if (network.Type == ConnectivityType.Mobile)
            {
                TelephonyManager manager = (TelephonyManager)this.mvxAndroidCurrentTopActivity.Activity.GetSystemService(Context.TelephonyService);
                return manager.NetworkOperatorName;
            }
            return this.GetNetworkInfo().Type.ToString();
        }
    }
}