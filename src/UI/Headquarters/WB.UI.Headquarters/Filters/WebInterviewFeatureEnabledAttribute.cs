using System.Web.Mvc;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Filters
{
    public class WebInterviewFeatureEnabledAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!LegacyOptions.WebInterviewEnabled)
            {
                filterContext.Result = new HttpNotFoundResult("Web interview feature is not enabled");
            }
        }
    }
}