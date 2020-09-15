using System.IO;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public interface IPayload
    {
        string Endpoint { get;  }
        byte[] Bytes { get; }
        long Id { get; }
        Stream Stream { get; }
        PayloadType Type { get; }

        void ReadStream();
        byte[] BytesFromStream { get; }
    }
}
