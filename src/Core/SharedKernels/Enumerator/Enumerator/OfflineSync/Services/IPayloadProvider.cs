using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public interface IPayloadProvider
    {
        IPayload AsBytes(byte[] bytes);
        IPayload AsStream(byte[] bytes);
    }
}