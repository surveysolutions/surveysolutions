using System.Web.Mvc;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NotFound()
        {
            return View();
        }

        public ActionResult AccessDenied()
        {
            return View();
        }

        public ActionResult Forbidden()
        {
            return this.View();
        }

        public ActionResult WebsitePathIsTooLong()
        {
            return this.View();
        }
    }
}
