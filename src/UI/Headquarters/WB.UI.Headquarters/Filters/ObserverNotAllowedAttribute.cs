using System.Web;
using System.Web.Mvc;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.Security;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Filters
{
    public class ObserverNotAllowedAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (HttpContext.Current.User.Identity.IsObserver())
            {
                throw new HttpException(403, Strings.ObserverNotAllowed);
            }

            base.OnActionExecuting(filterContext);
        }
    }
}