using Cirrious.MvvmCross.Plugins.Network.Reachability;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.UI.QuestionnaireTester.Implementation.Services
{
    internal class NetworkService : INetworkService
    {
        private readonly IMvxReachability mvxReachability;
        private readonly IRestServiceSettings restServiceSettings;

        public NetworkService(IMvxReachability mvxReachability, IRestServiceSettings restServiceSettings)
        {
            this.mvxReachability = mvxReachability;
            this.restServiceSettings = restServiceSettings;
        }

        public bool IsNetworkEnabled()
        {
            return this.mvxReachability.IsHostReachable(this.restServiceSettings.Endpoint);
        }
    }
}