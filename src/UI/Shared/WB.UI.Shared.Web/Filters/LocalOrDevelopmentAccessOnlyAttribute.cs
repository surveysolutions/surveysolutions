using System.Web;
using System.Web.Mvc;
using WB.UI.Shared.Web.Settings;

namespace WB.UI.Shared.Web.Filters
{
    public class LocalOrDevelopmentAccessOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsLocal && !CoreSettings.IsDevelopmentEnvironment)
            {
                throw new HttpException(403, "you are not allowed to see contents of this page under current configuration");
            }
            base.OnActionExecuting(filterContext);
        }
    }
}