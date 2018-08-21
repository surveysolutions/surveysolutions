using System.Web.Mvc;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Headquarters.Controllers;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [AllowAnonymous]
    public class HealthCheckController : BaseController
    {
        public HealthCheckController(ICommandService commandService, ILogger logger)
            : base(commandService, logger)
        {
        }

        public ActionResult Index()
        {
            return this.View();
        }
    }
}
