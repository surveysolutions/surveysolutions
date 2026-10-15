using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Maps
{
    [DisallowConcurrentExecution]
    [DisplayName("Map file cleanup job"), Category("Cleanup")]
    public class MapFileCleanupJob : IJob
    {
        private readonly IPlainStorageAccessor<MapFileDeletionRequest> requests;
        private readonly IMapStorageService mapStorageService;
        private readonly ILogger<MapFileCleanupJob> logger;

        public MapFileCleanupJob(
            IPlainStorageAccessor<MapFileDeletionRequest> requests,
            IMapStorageService mapStorageService,
            ILogger<MapFileCleanupJob> logger)
        {
            this.requests = requests;
            this.mapStorageService = mapStorageService;
            this.logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var pendingRequests = requests.Query(query => query.ToArray());

            foreach (var request in pendingRequests)
            {
                try
                {
                    await mapStorageService.RemoveMapFileIfNotInUseAsync(
                        request.FileName, request.StorageKey ?? request.FileName);
                    requests.Remove(request.Id);
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Failed to delete map file {FileName}; cleanup will be retried",
                        request.FileName);
                }
            }
        }
    }
}
