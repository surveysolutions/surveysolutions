using System.Web.Mvc;

using WB.Core.Infrastructure;

namespace Web.Supervisor.Controllers
{
    public class ControlPanelController : Controller
    {
        private readonly IReadLayerAdministrationService readLayerAdministrationService;

        public ControlPanelController(IReadLayerAdministrationService readLayerAdministrationService)
        {
            this.readLayerAdministrationService = readLayerAdministrationService;
        }

        public ActionResult ReadLayer()
        {
            return this.View();
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public string GetReadLayerStatus()
        {
            return this.readLayerAdministrationService.GetReadableStatus();
        }

        public ActionResult RebuildReadLayer()
        {
            this.readLayerAdministrationService.RebuildAllViewsAsync();

            return this.RedirectToAction("ReadLayer");
        }

        public ActionResult StopReadLayerRebuilding()
        {
            this.readLayerAdministrationService.StopAllViewsRebuilding();

            return this.RedirectToAction("ReadLayer");
        }
    }
}