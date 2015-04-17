using System.Web;
using System.Web.Mvc;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.Security;


namespace WB.Core.SharedKernels.SurveyManagement.Web.Filters
{
    public class ObserverNotAllowedAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (HttpContext.Current.User.Identity.IsObserver())
            {
                throw new HttpException(403, "Observer is not allowed to perform this action");
            }
            
            base.OnActionExecuting(filterContext);
        }
    }
}
