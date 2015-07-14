using System;
using Cirrious.MvvmCross.Plugins.Network.Reachability;
using WB.Core.BoundedContexts.Tester.Infrastructure;

namespace WB.Core.Infrastructure.Android.Implementation.Services.Network
{
    internal class NetworkService : INetworkService
    {
        private readonly IMvxReachability mvxReachability;
        private readonly ISettingsProvider settingsProvider;

        public NetworkService(IMvxReachability mvxReachability, ISettingsProvider settingsProvider)
        {
            if(mvxReachability == null) throw new ArgumentNullException("mvxReachability");
            if(settingsProvider == null) throw new ArgumentNullException("settingsProvider");

            this.mvxReachability = mvxReachability;
            this.settingsProvider = settingsProvider;
        }

        public bool IsNetworkEnabled()
        {
            return this.mvxReachability.IsHostReachable(this.settingsProvider.Endpoint);
        }
    }
}