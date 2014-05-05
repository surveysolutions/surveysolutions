using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WB.UI.Supervisor.Filters
{
    public class LongWebsiteDirectoryPathFilter : ActionFilterAttribute
    {
        private const int MaxDirectoryPathLength = 248;
        private const int AppDataDirectoryPathLength = 8;
        private const int ExportDirectoryPathLength = 12;
        private const int SpecifyExportDirectoryPathLength = 54;
        private const string ErrorControllerName = "Error";
        private const string WebsitePathIsTooLongActionName = "WebsitePathIsTooLong";

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            var actionName = filterContext.ActionDescriptor.ActionName;

            var isPathLimitExceed = HttpRuntime.AppDomainAppPath.Length >
                         (MaxDirectoryPathLength - AppDataDirectoryPathLength - ExportDirectoryPathLength -
                          SpecifyExportDirectoryPathLength);

            if ((controllerName != ErrorControllerName) && (actionName != WebsitePathIsTooLongActionName) && isPathLimitExceed)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        {"controller", ErrorControllerName},
                        {"action", WebsitePathIsTooLongActionName}
                    });
            }
        }
    }
}