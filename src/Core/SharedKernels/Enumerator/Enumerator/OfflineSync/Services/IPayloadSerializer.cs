using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public interface IPayloadSerializer
    {
        Task<T> FromPayloadAsync<T>(byte[] payload);
        Task<byte[]> ToPayloadAsync<T>(T message);
    }
}
