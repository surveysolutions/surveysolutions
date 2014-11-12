using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Designer.Controllers
{
    [LocalOrDevelopmentAccessOnly]
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

        public ActionResult Index()
        {
            return this.View();
        }

        public ActionResult NConfig()
        {
            return this.View();
        }

        public ActionResult ReadLayer()
        {
            return this.RedirectToActionPermanent("ReadSide");
        }

        public ActionResult ReadSide()
        {
            return this.View(this.ReadSideAdministrationService.GetAllAvailableHandlers());
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public string GetReadSideStatus()
        {
            return this.ReadSideAdministrationService.GetReadableStatus();
        }

        public ActionResult RebuildReadSidePartially(string[] handlers, int skipEvents = 0)
        {
            this.ReadSideAdministrationService.RebuildViewsAsync(handlers, skipEvents);
            this.TempData["InProgress"] = true;
            this.TempData["CheckedHandlers"] = handlers;
            this.TempData["SkipEvents"] = skipEvents;
            return this.RedirectToAction("ReadSide");
        }

        public ActionResult RebuildReadSidePartiallyForEventSources(string[] handlers, string eventSourceIds)
        {
            var sourceIds =
                eventSourceIds.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).Select(e => Guid.Parse(e.Trim())).ToArray();
            this.ReadSideAdministrationService.RebuildViewForEventSourcesAsync(handlers, sourceIds);

            this.TempData["InProgress"] = true;
            this.TempData["CheckedHandlers"] = handlers;
            this.TempData["EventSources"] = eventSourceIds;
            return this.RedirectToAction("ReadSide");
        }

        public ActionResult RebuildReadSide(int skipEvents = 0)
        {
            this.ReadSideAdministrationService.RebuildAllViewsAsync(skipEvents);
            this.TempData["InProgress"] = true;
            this.TempData["SkipEvents"] = skipEvents;
            return this.RedirectToAction("ReadSide");
        }

        public ActionResult StopReadSideRebuilding()
        {
            this.ReadSideAdministrationService.StopAllViewsRebuilding();

            return this.RedirectToAction("ReadSide");
        }

        public ActionResult NCalcToCSharp()
        {
            return this.View();
        }

        public ActionResult ExpressionGeneration()
        {
            return this.View();
        }
    }
}