using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers
{
    [ValidateInput(false)]
    [AuthorizeOr403(Roles = "Administrator, Headquarter, Supervisor, Observer")]
    public class InterviewersController : TeamController
    {
        public InterviewersController(ICommandService commandService, 
            ILogger logger, IAuthorizedUser authorizedUser, HqUserManager userManager)
            : base(commandService, logger, authorizedUser, userManager)
        {
        }
       
        public ActionResult Index(InterviewersFilter interviewersFilter)
        {
            this.ViewBag.ActivePage = MenuItem.Interviewers;

            InterviewersModel pageModel = new InterviewersModel();
            return this.View(pageModel);
        }
    }
}