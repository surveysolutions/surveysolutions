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

        public NearbyPayloadCallback(Action<IPayload> onPayloadReceived, Action<NearbyPayloadTransferUpdate> onPayloadTransferUpdate)
        {
            OnPayloadReceived = onPayloadReceived;
            OnPayloadTransferUpdate = onPayloadTransferUpdate;
        }

        public Action<IPayload> OnPayloadReceived { get; set; }
        public Action<NearbyPayloadTransferUpdate> OnPayloadTransferUpdate { get; set; }
    }
}
