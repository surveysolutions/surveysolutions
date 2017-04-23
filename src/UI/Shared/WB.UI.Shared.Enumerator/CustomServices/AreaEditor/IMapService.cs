using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WB.UI.Shared.Enumerator.CustomServices.AreaEditor
{
    public interface IMapService
    {
        Dictionary<string, string> GetAvailableMaps();
        Task SyncMaps(CancellationToken cancellationToken);
    }
}