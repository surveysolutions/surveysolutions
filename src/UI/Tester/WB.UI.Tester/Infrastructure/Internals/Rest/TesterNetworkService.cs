using System;
using Android.Net;
using Cirrious.CrossCore.Droid.Platform;
using Cirrious.MvvmCross.Plugins.Network.Reachability;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.UI.Tester.Infrastructure.Internals.Rest
{
    internal class TesterNetworkService : ITesterNetworkService
    {
        private readonly IMvxReachability mvxReachability;
        private readonly IRestServiceSettings settings;
        private readonly IMvxAndroidCurrentTopActivity mvxAndroidCurrentTopActivity;

        public TesterNetworkService(IMvxReachability mvxReachability, IRestServiceSettings settings, IMvxAndroidCurrentTopActivity mvxAndroidCurrentTopActivity)
        {
            if(mvxReachability == null) throw new ArgumentNullException("mvxReachability");
            if(settings == null) throw new ArgumentNullException("settings");

            this.mvxReachability = mvxReachability;
            this.settings = settings;
            this.mvxAndroidCurrentTopActivity = mvxAndroidCurrentTopActivity;
        }

        public bool IsEndpointReachable()
        {
            return this.mvxReachability.IsHostReachable(this.settings.Endpoint);
        }

        public bool IsNetworkEnabled()
        {
            var connectivityManager = (ConnectivityManager)this.mvxAndroidCurrentTopActivity.Activity.GetSystemService(Android.Content.Context.ConnectivityService);
            var activeConnection = connectivityManager.ActiveNetworkInfo;

            return activeConnection != null && activeConnection.IsConnected;
        }
    }
}