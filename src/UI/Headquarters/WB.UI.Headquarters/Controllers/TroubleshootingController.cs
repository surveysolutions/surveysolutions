using System;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Models.Troubleshooting;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [Authorize]
    public class TroubleshootingController : Controller
    {
        private readonly IAuthorizedUser authorizedUser;

        public TroubleshootingController(IAuthorizedUser authorizedUser)
        {
            this.authorizedUser = authorizedUser;
        }

        public ActionResult Index()
        {
            this.ViewBag.ActivePage = MenuItem.Troubleshooting;
            return View();
        }

        public ActionResult CensusInterviews()
        {
            this.ViewBag.ActivePage = MenuItem.Troubleshooting;
            return View(new CensusInterviewsModel
            {
            });
        }

        public ActionResult DataIsMissing(string id)
        {
            this.ViewBag.ActivePage = MenuItem.Troubleshooting;
            return View(new DataIsMissingModel
            {
                InterviewId = id
            });
        }
    }
}