using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.ReadSide;

namespace WB.UI.Shared.Web.Filters
{
    public abstract class AbstractMaintenanceFilter : ActionFilterAttribute
    {
        private IReadSideStatusService readSideStatusService
        {
            get { return ServiceLocator.Current.GetInstance<IReadSideStatusService>(); }
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (this.IsControlPanelController(filterContext.Controller)) return;

            if (!(this.IsMaintenanceController(filterContext.Controller)) && this.readSideStatusService.AreViewsBeingRebuiltNow())
            {
                filterContext.Result =
                    new RedirectToRouteResult(
                        new RouteValueDictionary(
                            new
                            {
                                controller = "Maintenance",
                                action = "WaitForReadLayerRebuild",
                                returnUrl = filterContext.RequestContext.HttpContext.Request.Url.ToString()
                            }));
            }
        }

        protected abstract bool IsMaintenanceController(ControllerBase controller);
        protected abstract bool IsControlPanelController(ControllerBase controller);
    }
}