using System;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Headquarters.Controllers;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    public class WebInterviewController : BaseController
    {
        public WebInterviewController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger)
            : base(commandService, globalInfo, logger)
        {

        }

        public ActionResult Index(string id)
        {
            return this.View();
        }
    }
}