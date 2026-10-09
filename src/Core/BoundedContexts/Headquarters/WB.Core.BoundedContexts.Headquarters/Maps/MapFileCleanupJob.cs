using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Maps
{
    [DisallowConcurrentExecution]
    [DisplayName("Map file cleanup job"), Category("Cleanup")]
    public class MapFileCleanupJob : IJob
    {
        private readonly IPlainStorageAccessor<MapFileDeletionRequest> requests;
        private readonly IPlainStorageAccessor<MapBrowseItem> maps;
        private readonly IMapStorageService mapStorageService;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<MapFileCleanupJob> logger;

        public MapFileCleanupJob(
            IPlainStorageAccessor<MapFileDeletionRequest> requests,
            IPlainStorageAccessor<MapBrowseItem> maps,
            IMapStorageService mapStorageService,
            IUnitOfWork unitOfWork,
            ILogger<MapFileCleanupJob> logger)
        {
            this.requests = requests;
            this.maps = maps;
            this.mapStorageService = mapStorageService;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var pendingRequests = requests.Query(query => query.ToArray());
            var deletedAny = false;

            foreach (var request in pendingRequests)
            {
                try
                {
                    if (maps.GetById(request.FileName) != null)
                    {
                        requests.Remove(request.Id);
                        deletedAny = true;
                        continue;
                    }

                    await mapStorageService.RemoveMapFileAsync(request.FileName);
                    requests.Remove(request.Id);
                    deletedAny = true;
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Failed to delete map file {FileName}; cleanup will be retried",
                        request.FileName);
                }
            }

            if (!deletedAny)
                return;

            unitOfWork.AcceptChanges();
            unitOfWork.Complete();
        }
    }
}
