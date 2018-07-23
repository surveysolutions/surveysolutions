using System;
using System.Diagnostics.CodeAnalysis;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Entities
{
    [ExcludeFromCodeCoverage] // because instantiated from UI projects
    public class NearbyConnectionLifeCycleCallback
    {
        public NearbyConnectionLifeCycleCallback(
            Action<NearbyConnectionInfo> onConnectionInitiated, 
            Action<NearbyConnectionResolution> onConnectionResult, 
            Action<string> onDisconnected)
        {
            OnConnectionInitiated = onConnectionInitiated;
            OnConnectionResult = onConnectionResult;
            OnDisconnected = onDisconnected;
        }

        public Action<NearbyConnectionInfo> OnConnectionInitiated { get; set; }
        public Action<NearbyConnectionResolution> OnConnectionResult { get; set; }
        public Action<string> OnDisconnected { get; set; }
    }
}
