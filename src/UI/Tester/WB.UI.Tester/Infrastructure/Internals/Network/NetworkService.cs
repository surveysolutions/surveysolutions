using System;
using Cirrious.MvvmCross.Plugins.Network.Reachability;
using WB.Core.BoundedContexts.Tester.Infrastructure;

namespace WB.UI.Tester.Infrastructure.Internals.Network
{
    internal class NetworkService : INetworkService
    {
        private readonly IMvxReachability mvxReachability;
        private readonly ITesterSettings settings;

        public NetworkService(IMvxReachability mvxReachability, ITesterSettings settings)
        {
            if(mvxReachability == null) throw new ArgumentNullException("mvxReachability");
            if(settings == null) throw new ArgumentNullException("settings");

            this.mvxReachability = mvxReachability;
            this.settings = settings;
        }

        public bool IsNetworkEnabled()
        {
            return this.mvxReachability.IsHostReachable(this.settings.Endpoint);
        }
    }
}