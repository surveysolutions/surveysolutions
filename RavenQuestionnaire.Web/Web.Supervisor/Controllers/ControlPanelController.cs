using System.Web.Mvc;

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;

namespace Web.Supervisor.Controllers
{
    [AllowAnonymous]
    public class ControlPanelController : Controller
    {
        private readonly IReadSideAdministrationService readSideAdministrationService;

        public ControlPanelController(IReadSideAdministrationService readSideAdministrationService)
        {
            this.readSideAdministrationService = readSideAdministrationService;
        }

        public ActionResult ReadLayer()
        {
            return this.View();
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public string GetReadLayerStatus()
        {
            return this.readSideAdministrationService.GetReadableStatus();
        }

        public ActionResult RebuildReadLayer()
        {
            this.readSideAdministrationService.RebuildAllViewsAsync();

            return this.RedirectToAction("ReadLayer");
        }

        public ActionResult StopReadLayerRebuilding()
        {
            this.readSideAdministrationService.StopAllViewsRebuilding();

            return this.RedirectToAction("ReadLayer");
        }
    }
}