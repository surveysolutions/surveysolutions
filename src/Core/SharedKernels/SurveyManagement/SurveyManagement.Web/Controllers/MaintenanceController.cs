using System.Web.Mvc;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    public class MaintenanceController : Controller
    {
        public ActionResult WaitForReadLayerRebuild(string returnUrl)
        {
            return this.View(model: returnUrl);
        }
    }
}