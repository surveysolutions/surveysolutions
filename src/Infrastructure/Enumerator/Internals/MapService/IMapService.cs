using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Infrastructure.Shared.Enumerator.Internals.MapService
{
    public interface IMapService
    {
        Dictionary<string, string> GetAvailableMaps();
        Task SyncMaps(CancellationToken cancellationToken);
    }
}