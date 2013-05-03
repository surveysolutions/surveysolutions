namespace WB.UI.Designer.Filters
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using Main.Core;

    /// <summary>
    /// Filter which ensures that read layer is built before action is executed.
    /// </summary>
    public class RequiresReadLayerFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (NcqrsInit.IsReadLayerBuilt)
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            ((Action)NcqrsInit.EnsureReadLayerIsBuilt)
                .BeginInvoke(null, null);

            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
            {
                { "controller", "Maintenance" },
                { "action", "ReadLayer" },
                { "returnUrl", filterContext.RequestContext.HttpContext.Request.Url }
            });
        }
    }
}