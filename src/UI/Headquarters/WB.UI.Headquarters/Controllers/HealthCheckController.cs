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
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.API
{
    [AllowAnonymousAttribute] 
    public class HealthCheckController : BaseController
    {
        private readonly IReadSideAdministrationService readSideAdministrationService;
        private readonly IEventStore eventStore;
        private readonly IDatabaseHealthCheck databaseHealthCheck;
        private readonly IEventStoreHealthCheck eventStoreHealthCheck;

        public HealthCheckController(ICommandService commandService, IGlobalInfoProvider provider, ILogger logger,
            IDatabaseHealthCheck databaseHealthCheck, IEventStoreHealthCheck eventStoreHealthCheck, IEventStore eventStore,
            IReadSideAdministrationService readSideAdministrationService)
            : base(commandService, provider, logger)
        {
            this.eventStoreHealthCheck = eventStoreHealthCheck;
            this.databaseHealthCheck = databaseHealthCheck;
            this.eventStore = eventStore;
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
            var readSideStatus = readSideAdministrationService.GetRebuildStatus();

            return new HealthCheckModel()
            {
                DatabaseConnectionStatus = databaseHealthCheckResult,
                EventstoreConnectionStatus = eventStoreHealthCheckResult,
                ReadSideServiceStatus = readSideStatus,

                Status = databaseHealthCheckResult.Status == eventStoreHealthCheckResult.Status
                            ? databaseHealthCheckResult.Status
                            : HealthCheckStatus.Down
            };
        }
    }
}