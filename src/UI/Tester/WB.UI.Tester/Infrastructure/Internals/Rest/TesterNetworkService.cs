using System;
using Cirrious.MvvmCross.Plugins.Network.Reachability;

namespace WB.UI.Tester.Infrastructure.Internals.Rest
{
    internal class TesterNetworkService : ITesterNetworkService
    {
        private readonly IMvxReachability mvxReachability;
        private readonly ITesterSettings settings;

        public TesterNetworkService(IMvxReachability mvxReachability, ITesterSettings settings)
        {
            if(mvxReachability == null) throw new ArgumentNullException("mvxReachability");
            if(settings == null) throw new ArgumentNullException("settings");

            this.mvxReachability = mvxReachability;
            this.settings = settings;
        }

        public bool IsEndpointReachable()
        {
            return this.mvxReachability.IsHostReachable(this.settings.Endpoint);
        }
    }
}