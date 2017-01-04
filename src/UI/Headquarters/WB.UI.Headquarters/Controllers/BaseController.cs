using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Shared.Web.Controllers;

namespace WB.UI.Headquarters.Controllers
{
    public abstract class BaseController : BaseMessageDisplayController
    {
        protected readonly ICommandService CommandService;
        protected readonly IGlobalInfoProvider GlobalInfo;

        protected readonly ILogger Logger;

        protected BaseController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger)
        {
            this.CommandService = commandService;
            this.GlobalInfo = globalInfo;
            this.Logger = logger;
        }
    }
}