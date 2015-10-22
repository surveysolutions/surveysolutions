using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.ReadSide;

namespace WB.UI.Shared.Web.Filters
{
    public abstract class AbstractMaintenanceFilter : ActionFilterAttribute
    {
        private IReadSideStatusService ReadSideStatusService => ServiceLocator.Current.GetInstance<IReadSideStatusService>();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (this.IsControlPanelController(filterContext.Controller)) return;
            if (this.IsMaintenanceController(filterContext.Controller)) return;

            if (this.ReadSideStatusService.AreViewsBeingRebuiltNow())
            {
                filterContext.Result =
                    new RedirectToRouteResult(
                        new RouteValueDictionary(
                            new
                            {
                                controller = "Maintenance",
                                action = "WaitForReadSideRebuild",
                                returnUrl = filterContext.RequestContext.HttpContext.Request.Url.ToString()
                            }));
            }
        }

        protected abstract bool IsMaintenanceController(ControllerBase controller);
        protected abstract bool IsControlPanelController(ControllerBase controller);
    }
}