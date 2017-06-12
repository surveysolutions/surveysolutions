using System.Threading;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IMapSynchronizer
    {
        Task SyncMaps(string workingDirectory, CancellationToken cancellationToken);
    }
}