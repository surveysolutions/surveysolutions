using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IMapSynchronizer
    {
        Task<List<MapView>> GetMapList(CancellationToken cancellationToken);
        Task<byte[]> GetMapContent(string url, CancellationToken token);
    }
}