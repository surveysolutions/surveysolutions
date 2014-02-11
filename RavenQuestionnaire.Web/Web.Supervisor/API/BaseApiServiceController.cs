using System.Web.Http;
using WB.Core.GenericSubdomains.Logging;

namespace Web.Supervisor.API
{
    public abstract class BaseApiServiceController : ApiController
    {
        protected readonly ILogger Logger;
        protected int maxPageSize = 40;

        protected BaseApiServiceController(ILogger logger)
        {
            this.Logger = logger;

        }
    }
}




