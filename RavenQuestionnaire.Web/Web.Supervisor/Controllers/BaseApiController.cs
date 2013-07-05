using WB.Core.GenericSubdomains.Logging;

namespace Web.Supervisor.Controllers
{
    using System.Web.Http;

    using Ncqrs.Commanding.ServiceModel;

    using Questionnaire.Core.Web.Helpers;

    public abstract class BaseApiController : ApiController
    {
        protected readonly ICommandService CommandService;
        protected readonly IGlobalInfoProvider GlobalInfo;

        protected readonly ILogger Logger;

        protected BaseApiController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger)
        {
            this.CommandService = commandService;
            this.GlobalInfo = globalInfo;
            this.Logger = logger;
        }
    }
}