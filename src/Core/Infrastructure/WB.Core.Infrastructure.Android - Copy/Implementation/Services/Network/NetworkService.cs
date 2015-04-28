using System;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Plugins.Network.Reachability;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;

namespace WB.Core.Infrastructure.Android.Implementation.Services.Network
{
    internal class NetworkService : INetworkService
    {
        private readonly IMvxReachability mvxReachability;
        private readonly RestServiceSettings restServiceSettings;

        public NetworkService(IMvxReachability mvxReachability, RestServiceSettings restServiceSettings)
        {
            if(mvxReachability == null) throw new ArgumentNullException("mvxReachability");
            if(restServiceSettings == null) throw new ArgumentNullException("restServiceSettings");

            this.mvxReachability = mvxReachability;
            this.restServiceSettings = restServiceSettings;
        }

        public Task<bool> IsNetworkEnabledAsync()
        {
            return Task.Run(() => this.mvxReachability.IsHostReachable(this.restServiceSettings.Endpoint));
        }
    }
}