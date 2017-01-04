using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [ValidateInput(false)]
    [Authorize(Roles = "Administrator, Headquarter, Supervisor, Observer")]
    public class InterviewersController : TeamController
    {
        public InterviewersController(ICommandService commandService, IGlobalInfoProvider globalInfo, 
            ILogger logger, IUserViewFactory userViewFactory, IPasswordHasher passwordHasher)
            : base(commandService, globalInfo, logger, userViewFactory, passwordHasher)
        {
        }

        public ActionResult Index(InterviewersFilter filter)
        {
            this.ViewBag.ActivePage = MenuItem.Interviewers;

            InterviewersModel pageModel = new InterviewersModel();
            return this.View(pageModel);
        }
    }
}