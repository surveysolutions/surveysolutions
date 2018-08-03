using System.IO;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.UI.Shared.Enumerator.OfflineSync.Services.Entities;

namespace WB.UI.Shared.Enumerator.OfflineSync.Services.Implementation
{
    class PayloadProvider : IPayloadProvider
    {
        public IPayload AsBytes(byte[] bytes, string endpoint) => Payload.FromBytes(endpoint, bytes);

        public IPayload AsStream(byte[] bytes, string endpoint) => Payload.FromStream(endpoint, new MemoryStream(bytes));
    }
}
