using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public interface IPayloadSerializer
    {
        T FromPayload<T>(byte[] payload);
        byte[] ToPayload<T>(T message);
    }
}
