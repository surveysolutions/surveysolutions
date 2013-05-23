namespace Web.Supervisor.Controllers
{
    using System.Web.Mvc;

    using Main.Core;

    public class MaintenanceController : Controller
    {
        public ActionResult WaitForReadLayerRebuild(string returnUrl)
        {
            return this.View(model: returnUrl);
        }

        public ActionResult RebuildReadLayer(string returnUrl)
        {
            NcqrsInit.EnsureReadLayerIsBuilt();

            return this.Redirect(returnUrl);
        }
    }
}