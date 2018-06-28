using System;
using Android.Gms.Nearby.Connection;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;

namespace WB.UI.Shared.Enumerator.OfflineSync.Entities
{
    internal class OnDiscoveryCallback : EndpointDiscoveryCallback
    {
        private readonly Action<string, NearbyDiscoveredEndpointInfo> foundEndpoint;
        private readonly Action<string> lostEndpoint;


        public OnDiscoveryCallback(Action<string, NearbyDiscoveredEndpointInfo> foundEndpoint, Action<string> lostEndpoint)
        {
            this.foundEndpoint = foundEndpoint;
            this.lostEndpoint = lostEndpoint;
        }

        public override void OnEndpointFound(string endpointId, DiscoveredEndpointInfo info)
        {
            foundEndpoint(endpointId, new NearbyDiscoveredEndpointInfo
            {
                EndpointName = info.EndpointName
            });
        }

        public override void OnEndpointLost(string endpointId)
        {
            lostEndpoint(endpointId);
        }
    }
}
