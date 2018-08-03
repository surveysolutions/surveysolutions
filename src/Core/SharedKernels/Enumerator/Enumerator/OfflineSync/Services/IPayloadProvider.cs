using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public interface IPayloadProvider
    {
        IPayload AsBytes(byte[] bytes, string endpoint);
        IPayload AsStream(byte[] bytes, string endpoint);
    }
}
