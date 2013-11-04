using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.ReadSide;

namespace WB.UI.Designer.Controllers
{
    [AllowAnonymous]
    public class ControlPanelController : Controller
    {
        private readonly IServiceLocator serviceLocator;

        public ControlPanelController(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        /// <remarks>
        /// Getting dependency via service location ensures that parts of control panel not using it will always work.
        /// E.g. If Raven connection fails to be established then NConfig info still be available.
        /// </remarks>
        private IReadSideAdministrationService ReadSideAdministrationService
        {
            get { return this.serviceLocator.GetInstance<IReadSideAdministrationService>(); }
        }

        public ActionResult NConfig()
        {
            return this.View();
        }

        public ActionResult ReadLayer()
        {
            return this.View(this.ReadSideAdministrationService.GetAllAvailableHandlers());
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public string GetReadLayerStatus()
        {
            return this.ReadSideAdministrationService.GetReadableStatus();
        }

        public ActionResult RebuildReadLayerPartially(string[] handlers)
        {
            this.ReadSideAdministrationService.RebuildViewsAsync(handlers);
            this.TempData["CheckedHandlers"] = handlers;
            return this.RedirectToAction("ReadLayer");
        }

        public ActionResult RebuildReadLayer()
        {
            this.ReadSideAdministrationService.RebuildAllViewsAsync();

            return this.RedirectToAction("ReadLayer");
        }

        public ActionResult StopReadLayerRebuilding()
        {
            this.ReadSideAdministrationService.StopAllViewsRebuilding();

            return this.RedirectToAction("ReadLayer");
        }
    }
}