using System.IO;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public interface IPayload
    {
        byte[] Bytes { get; }
        long Id { get; }
        Stream Stream { get; }
        PayloadType Type { get; }

        Task ReadStreamAsync();
        Task<byte[]> BytesFromStream { get; }
    }
}
