﻿using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public static class NearbyEvent
    {
        public class EndpointFound : INearbyEvent
        {
            public string Endpoint { get; }
            public NearbyDiscoveredEndpointInfo EndpointInfo { get; }

            public EndpointFound(string endpoint, NearbyDiscoveredEndpointInfo endpointInfo)
            {
                Endpoint = endpoint;
                EndpointInfo = endpointInfo;
            }
        }

        public class EndpointLost : INearbyEvent
        {
            public EndpointLost(string endpoint)
            {
                Endpoint = endpoint;
            }

            public string Endpoint { get; }
        }

        public class Disconnected : INearbyEvent
        {
            public Disconnected(string endpoint, string name)
            {
                Endpoint = endpoint;
                Name = name;
            }

            public string Endpoint { get; }
            public string Name { get; }
        }

        public class Connected : INearbyEvent
        {
            public string Endpoint { get; }
            public string Name { get; }

            public Connected(string endpoint, string name)
            {
                Endpoint = endpoint;
                Name = name;
            }
        }

        public class InitiatedConnection : INearbyEvent
        {
            public string Endpoint { get; }
            public NearbyConnectionInfo Info { get; }

            public InitiatedConnection(string endpoint, NearbyConnectionInfo info)
            {
                Endpoint = endpoint;
                Info = info;
            }
        }
    }
}
