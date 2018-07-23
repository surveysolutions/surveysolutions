using System;
using System.Threading;
using Android.Gms.Nearby.Connection;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;

namespace WB.UI.Shared.Enumerator.OfflineSync.Entities
{
    internal class OnDiscoveryCallback : EndpointDiscoveryCallback
    {
        private readonly Action<NearbyDiscoveredEndpointInfo> foundEndpoint;
        private readonly Action<string> lostEndpoint;
        private readonly CancellationToken cancellationToken;

        public OnDiscoveryCallback(Action<NearbyDiscoveredEndpointInfo> foundEndpoint, Action<string> lostEndpoint, CancellationToken cancellationToken)
        {
            this.foundEndpoint = foundEndpoint;
            this.lostEndpoint = lostEndpoint;
            this.cancellationToken = cancellationToken;
        }

        public override void OnEndpointFound(string endpoint, DiscoveredEndpointInfo info)
        {
            foundEndpoint(new NearbyDiscoveredEndpointInfo
            {
                Endpoint = endpoint,
                EndpointName = info.EndpointName,
                CancellationToken = cancellationToken
            });
        }

        public override void OnEndpointLost(string endpointId)
        {
            lostEndpoint(endpointId);
        }
    }
}
