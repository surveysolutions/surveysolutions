using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
using WB.Core.GenericSubdomains.Logging;

namespace Web.Supervisor.API
{
    [Authorize(Roles = "Supervisor")]
    public class HqSynchronizationController : BaseApiServiceController
    {
        private readonly SynchronizationContext synchronizationContext;

        public HqSynchronizationController(ILogger logger,
            SynchronizationContext synchronizationContext)
            : base(logger)
        {
            this.synchronizationContext = synchronizationContext;
        }

        [Route("apis/syncStatus", Name = "api.SyncStatus")]
        [HttpGet]
        public HttpResponseMessage GetSyncStatus()
        {
            SynchronizationStatus synchronizationStatus = this.synchronizationContext.GetPersistedStatus();

            HttpResponseMessage httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK, synchronizationStatus ?? new SynchronizationStatus());
            return httpResponseMessage;
        }
    }
}