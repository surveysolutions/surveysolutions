namespace WB.UI.Shared.Web.Filters
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Web.Routing;

    using Main.Core;

    using WB.UI.Shared.Log;

    /// <summary>
    /// Filter which ensures that read layer is built before action is executed.
    /// </summary>
    /// <remarks>
    /// This filter requires existance of MaintenanceController with ReadLayer action which accepts returnUrl string parameter.
    /// </remarks>
    public class RequiresReadLayerFilter : ActionFilterAttribute
    {
        private readonly ILog Logger;

        public RequiresReadLayerFilter(ILog logger)
        {
            this.Logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (NcqrsInit.IsReadLayerBuilt)
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            Task.Run(() => this.RebuildreadLayer());

            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
            {
                { "controller", "Maintenance" },
                { "action", "ReadLayer" },
                { "returnUrl", filterContext.RequestContext.HttpContext.Request.Url }
            });
        }

        private void RebuildreadLayer()
        {
            try
            {
                NcqrsInit.EnsureReadLayerIsBuilt();
            }
            catch (Exception ex)
            {
                Logger.Error("Rebuild read layer error", ex);
            }
        }
    }
}