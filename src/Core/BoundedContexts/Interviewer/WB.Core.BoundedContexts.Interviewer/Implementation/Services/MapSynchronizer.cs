using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class MapSynchronizer : IMapSynchronizer
    {
        private readonly ISynchronizationService synchronizationService;

        public MapSynchronizer( ISynchronizationService synchronizationService)
        {
            this.synchronizationService = synchronizationService;
        }

        public Task<List<MapView>> GetMapList(CancellationToken cancellationToken)
        {
            return synchronizationService.GetMapList(cancellationToken);
        }

        public Task<byte[]> GetMapContent(string url, CancellationToken token)
        {
            return synchronizationService.GetMapContent(url, token);
        }
    }
}
