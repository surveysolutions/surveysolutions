using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.HealthCheck;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.Synchronization;
using WB.Core.Synchronization.SyncStorage;
using WB.UI.Headquarters.Models;
using WB.UI.Shared.Web.Filters;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api
{
    [LocalOrDevelopmentAccessOnly]
    public class ControlPanelApiController : ApiController
    {
        private readonly IReadSideAdministrationService readSideAdministrationService;
        private readonly IIncomingSyncPackagesQueue incomingSyncPackagesQueue;
        private readonly IUnhandledPackageStorage unhandledPackageStorage;
        private readonly IDatabaseHealthCheck databaseHealthCheck;
        private readonly IEventStoreHealthCheck eventStoreHealthCheck;
        private readonly IChunkReader chunkReader;
        private readonly IFolderPermissionChecker folderPermissionChecker;

        public ControlPanelApiController(IReadSideAdministrationService readSideAdministrationService,
            IIncomingSyncPackagesQueue incomingSyncPackagesQueue, IDatabaseHealthCheck databaseHealthCheck, 
            IEventStoreHealthCheck eventStoreHealthCheck, IUnhandledPackageStorage unhandledPackageStorage, 
            IChunkReader chunkReader, IFolderPermissionChecker folderPermissionChecker)
        {
            this.readSideAdministrationService = readSideAdministrationService;
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
            this.folderPermissionChecker = folderPermissionChecker;
            this.chunkReader = chunkReader;
            this.eventStoreHealthCheck = eventStoreHealthCheck;
            this.databaseHealthCheck = databaseHealthCheck;
            this.unhandledPackageStorage = unhandledPackageStorage;
        }

        public InterviewDetailsSchedulerViewModel InterviewDetails()
        {
            return new InterviewDetailsSchedulerViewModel()
            {
                Messages = new string[0]
            };
        }

        public int GetIncomingPackagesQueueLength()
        {
            return this.incomingSyncPackagesQueue.QueueLength;
        }

        public IEnumerable<ReadSideEventHandlerDescription> GetAllAvailableHandlers()
        {
            return this.readSideAdministrationService.GetAllAvailableHandlers();
        }

        public ReadSideStatus GetReadSideStatus()
        {
            return this.readSideAdministrationService.GetRebuildStatus();
        }

        [HttpPost]
        public void RebuildReadSide(RebuildReadSideInputViewModel model)
        {
            switch (model.RebuildType)
            {
                case RebuildReadSideType.All:
                    this.readSideAdministrationService.RebuildAllViewsAsync(model.NumberOfSkipedEvents);
                    break;
                case RebuildReadSideType.ByHandlers:
                    this.readSideAdministrationService.RebuildViewsAsync(model.ListOfHandlers, model.NumberOfSkipedEvents);
                    break;
                case RebuildReadSideType.ByHandlersAndEventSource:
                    this.readSideAdministrationService.RebuildViewForEventSourcesAsync(model.ListOfHandlers, model.ListOfEventSources);
                    break;
            }
        }

        [HttpPost]
        public void StopReadSideRebuilding()
        {
            this.readSideAdministrationService.StopAllViewsRebuilding();
        }

        public HealthCheckStatus GetStatus()
        {
            var healthCheckStatus = CollectHealthCheckStatus();
            return healthCheckStatus.Status;
        }

        public HealthCheckModel GetVerboseStatus()
        {
            return CollectHealthCheckStatus();
        }

        private HealthCheckModel CollectHealthCheckStatus()
        {
            var databaseHealthCheckResult = databaseHealthCheck.Check();
            var eventStoreHealthCheckResult = eventStoreHealthCheck.Check();
            var numberOfUnhandledPackages = unhandledPackageStorage.GetListOfUnhandledPackages().Count();
            var numberOfSyncPackagesWithBigSize = chunkReader.GetNumberOfSyncPackagesWithBigSize();
            var folderPermissionCheckResult = folderPermissionChecker.Check();
            var readSideStatus = readSideAdministrationService.GetRebuildStatus();

            return new HealthCheckModel()
            {
                DatabaseConnectionStatus = databaseHealthCheckResult,
                EventstoreConnectionStatus = eventStoreHealthCheckResult,
                NumberOfUnhandledPackages = numberOfUnhandledPackages,
                NumberOfSyncPackagesWithBigSize = numberOfSyncPackagesWithBigSize,
                FolderPermissionCheckResult = folderPermissionCheckResult,
                ReadSideServiceStatus = readSideStatus
            };
        }

    }
}