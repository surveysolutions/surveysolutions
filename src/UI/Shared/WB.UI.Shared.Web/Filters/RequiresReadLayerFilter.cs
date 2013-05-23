namespace WB.UI.Shared.Web.Filters
{
    using System;
    using System.Web.Mvc;
    using System.Web.Routing;

    using Main.Core;

    /// <summary>
    /// Filter which ensures that read layer is built before action is executed.
    /// </summary>
    /// <remarks>
    /// This filter requires existance of MaintenanceController with ReadLayer action which accepts returnUrl string parameter.
    /// </remarks>
    public class RequiresReadLayerFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (NcqrsInit.IsReadLayerBuilt)
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
            {
                { "controller", "Maintenance" },
                { "action", "WaitForReadLayerRebuild" },
                { "returnUrl", filterContext.RequestContext.HttpContext.Request.Url }
            });
        }
    }
}