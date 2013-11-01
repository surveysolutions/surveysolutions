using System.Web.Mvc;
using WB.Core.Infrastructure.ReadSide;

namespace WB.UI.Designer.Controllers
{
    [AllowAnonymous]
    public class ControlPanelController : Controller
    {
        private readonly IReadSideAdministrationService readSideAdministrationService;

        public ControlPanelController(IReadSideAdministrationService readSideAdministrationService)
        {
            this.readSideAdministrationService = readSideAdministrationService;
        }

        public ActionResult NConfig()
        {
            return this.View();
        }

        public ActionResult ReadLayer()
        {
            return this.View(this.readSideAdministrationService.GetAllAvailableHandlers());
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public string GetReadLayerStatus()
        {
            return this.readSideAdministrationService.GetReadableStatus();
        }

        public ActionResult RebuildReadLayerPartially(string[] handlers)
        {
            this.readSideAdministrationService.RebuildViewsAsync(handlers);
            this.TempData["CheckedHandlers"] = handlers;
            return this.RedirectToAction("ReadLayer");
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