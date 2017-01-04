using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;

namespace WB.UI.Headquarters.Controllers
{
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