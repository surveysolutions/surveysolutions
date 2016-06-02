using System.Web.Mvc;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [AllowAnonymous]
    public class HealthCheckController : BaseController
    {
        public HealthCheckController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger)
            : base(commandService, globalInfo, logger)
        {
        }

        public ActionResult Index()
        {
            return this.View();
        }
    }
}