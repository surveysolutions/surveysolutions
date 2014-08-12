using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.UI.Headquarters.Controllers;

namespace WB.UI.Headquarters.Filters
{
    public class MaintenanceFilter : ActionFilterAttribute
    {
        private IReadSideStatusService readSideStatusService
        {
            get { return ServiceLocator.Current.GetInstance<IReadSideStatusService>(); }
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.Controller is ControlPanelController) return;

            if (!(filterContext.Controller is MaintenanceController) && readSideStatusService.AreViewsBeingRebuiltNow())
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
    }
}