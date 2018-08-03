using System.Threading;
using Android.Gms.Nearby.Connection;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;

namespace WB.UI.Shared.Enumerator.OfflineSync.Entities
{
    internal class OnConnectionLifecycleCallback : ConnectionLifecycleCallback
    {
        private readonly NearbyConnectionLifeCycleCallback callback;
        private readonly CancellationToken cancellationToken;

        public OnConnectionLifecycleCallback(NearbyConnectionLifeCycleCallback callback, CancellationToken cancellationToken)
        {
            this.callback = callback;
            this.cancellationToken = cancellationToken;
        }

        public override void OnConnectionInitiated(string endpoint, ConnectionInfo connectionInfo)
            => this.callback.OnConnectionInitiated(new NearbyConnectionInfo
            {
                Endpoint = endpoint,
                CancellationToken = cancellationToken,
                EndpointName = connectionInfo.EndpointName,
                IsIncomingConnection = connectionInfo.IsIncomingConnection,
                AuthenticationToken = connectionInfo.AuthenticationToken
            });

        public override void OnConnectionResult(string endpoint, ConnectionResolution resolution)
            => callback.OnConnectionResult(new NearbyConnectionResolution
            {
                Endpoint = endpoint,
                CancellationToken = cancellationToken,
                IsSuccess = resolution.Status.IsSuccess,
                StatusCode = resolution.Status.StatusCode
            });

        public override void OnDisconnected(string endpoint) => callback.OnDisconnected(endpoint);
    }
}
