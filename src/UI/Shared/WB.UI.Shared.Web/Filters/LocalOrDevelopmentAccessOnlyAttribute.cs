using System.Web;
using System.Web.Mvc;
using WB.UI.Shared.Web.Settings;

namespace WB.UI.Shared.Web.Filters
{
    public class LocalOrDevelopmentAccessOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsLocal && !CoreSettings.IsUnderDevelopment)
            {
                throw new HttpException(404, "controller is missing with current web site configuration");
            }
            base.OnActionExecuting(filterContext);
        }
    }
}