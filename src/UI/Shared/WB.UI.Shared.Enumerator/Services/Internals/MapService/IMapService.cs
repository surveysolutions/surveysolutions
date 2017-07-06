using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WB.UI.Shared.Enumerator.Services.Internals.MapService
{
    public interface IMapService
    {
        List<MapDescription> GetAvailableMaps();
        Task SyncMaps(CancellationToken cancellationToken);
    }
}