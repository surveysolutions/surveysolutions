using Android.Gms.Nearby.Connection;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;

namespace WB.UI.Shared.Enumerator.OfflineSync.Entities
{
    internal class OnConnectionLifecycleCallback : ConnectionLifecycleCallback
    {
        private readonly NearbyConnectionLifeCycleCallback callback;

        public OnConnectionLifecycleCallback(NearbyConnectionLifeCycleCallback callback)
        {
            this.callback = callback;
        }

        public override void OnConnectionInitiated(string endpointId, ConnectionInfo connectionInfo)
        {
            this.callback.OnConnectionInitiated(endpointId, new NearbyConnectionInfo
            {
                EndpointName = connectionInfo.EndpointName,
                IsIncomingConnection = connectionInfo.IsIncomingConnection,
                AuthenticationToken  = connectionInfo.AuthenticationToken
            });
        }

        public override void OnConnectionResult(string endpointId, ConnectionResolution resolution)
        {
            callback.OnConnectionResult(endpointId, new NearbyConnectionResolution
            {
                IsSuccess = resolution.Status.IsSuccess,
                StatusCode = resolution.Status.StatusCode
            });
        }

        public override void OnDisconnected(string endpointId)
        {
            callback.OnDisconnected(endpointId);
        }
    }
}
