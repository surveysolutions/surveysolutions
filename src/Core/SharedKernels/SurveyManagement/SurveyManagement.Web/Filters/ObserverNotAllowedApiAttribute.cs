using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.Security;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Filters
{
    public class ObserverNotAllowedApiAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            if (HttpContext.Current.User.Identity.IsObserver())
            {
                filterContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)
                {
                    Content = new StringContent("Observer is not allowed to perform this action")
                };
            }
            else
                base.OnActionExecuting(filterContext);
        }
    }
}