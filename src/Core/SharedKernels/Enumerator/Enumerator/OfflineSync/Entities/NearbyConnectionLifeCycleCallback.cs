using System;
using System.Diagnostics.CodeAnalysis;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Entities
{
    [ExcludeFromCodeCoverage]
    public class NearbyConnectionLifeCycleCallback
    {
        public NearbyConnectionLifeCycleCallback(
            Action<string, NearbyConnectionInfo> onConnectionInitiated, 
            Action<string, NearbyConnectionResolution> onConnectionResult, 
            Action<string> onDisconnected)
        {
            OnConnectionInitiated = onConnectionInitiated;
            OnConnectionResult = onConnectionResult;
            OnDisconnected = onDisconnected;
        }

        public Action<string, NearbyConnectionInfo> OnConnectionInitiated { get; set; }
        public Action<string, NearbyConnectionResolution> OnConnectionResult { get; set; }
        public Action<string> OnDisconnected { get; set; }
    }
}
