using System;
using System.Diagnostics.CodeAnalysis;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Entities
{
    [ExcludeFromCodeCoverage]
    public class NearbyPayloadCallback
    {
        public NearbyPayloadCallback()
        {
            
        }

        public NearbyPayloadCallback(Action<string, IPayload> onPayloadReceived, Action<string, NearbyPayloadTransferUpdate> onPayloadTransferUpdate)
        {
            OnPayloadReceived = onPayloadReceived;
            OnPayloadTransferUpdate = onPayloadTransferUpdate;
        }

        public Action<string, IPayload> OnPayloadReceived { get; set; }
        public Action<string, NearbyPayloadTransferUpdate> OnPayloadTransferUpdate { get; set; }
    }
}
