using System.Web.Mvc;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [Authorize(Roles = "Interviewer")]
    public class InterviewerHqController : BaseController
    {
        public InterviewerHqController(ICommandService commandService, ILogger logger) : base(commandService, logger)
        {
        }

        public ActionResult CreateNew()
        {
            return View("Index");
        }

        public ActionResult Rejected()
        {
            return View("Index");
        }

        public ActionResult Completed()
        {
            return View("Index");
        }

        public ActionResult Started()
        {
            return View("Index");
        }
    }
}