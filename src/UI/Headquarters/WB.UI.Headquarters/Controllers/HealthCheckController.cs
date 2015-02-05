using System.Linq;
using System.Web.Http;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.HealthCheck;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.Storage.EventStore;
using WB.Core.Infrastructure.Storage.EventStore.Implementation;
using WB.Core.Infrastructure.Storage.Raven.Implementation;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization;
using WB.Core.Synchronization.SyncStorage;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.API
{
    [AllowAnonymousAttribute] 
    public class HealthCheckController : BaseController
    {
        private readonly IReadSideAdministrationService readSideAdministrationService;
        private readonly IIncomePackagesRepository incomePackagesRepository;
        private readonly IDatabaseHealthCheck databaseHealthCheck;
        private readonly IEventStoreHealthCheck eventStoreHealthCheck;
        private readonly IChunkReader chunkReader;

        public HealthCheckController(ICommandService commandService, IGlobalInfoProvider provider, ILogger logger,
            IDatabaseHealthCheck databaseHealthCheck, IEventStoreHealthCheck eventStoreHealthCheck, 
            IIncomePackagesRepository incomePackagesRepository, IChunkReader chunkReader,
            IReadSideAdministrationService readSideAdministrationService)
            : base(commandService, provider, logger)
        {
            this.chunkReader = chunkReader;
            this.eventStoreHealthCheck = eventStoreHealthCheck;
            this.databaseHealthCheck = databaseHealthCheck;
            this.incomePackagesRepository = incomePackagesRepository;
            this.readSideAdministrationService = readSideAdministrationService;
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
            var numberOfUnhandledPackages = incomePackagesRepository.GetListOfUnhandledPackages().Count();
            var numberOfSyncPackagesWithBigSize = chunkReader.GetNumberOfSyncPackagesWithBigSize();
            var readSideStatus = readSideAdministrationService.GetRebuildStatus();

            var status = HealthCheckStatus.Happy;

            if (databaseHealthCheckResult.Status != HealthCheckStatus.Happy
                || eventStoreHealthCheckResult.Status != HealthCheckStatus.Happy)
            {
                status = HealthCheckStatus.Down;
            }
            else if (numberOfUnhandledPackages > 0 || numberOfSyncPackagesWithBigSize > 0)
            {
                status = HealthCheckStatus.Warning;
            }

            return new HealthCheckModel()
            {
                DatabaseConnectionStatus = databaseHealthCheckResult,
                EventstoreConnectionStatus = eventStoreHealthCheckResult,
                NumberOfUnhandledPackages = numberOfUnhandledPackages,
                NumberOfSyncPackagesWithBigSize = numberOfSyncPackagesWithBigSize,
                ReadSideServiceStatus = readSideStatus,

                Status = status
            };
        }
    }
}