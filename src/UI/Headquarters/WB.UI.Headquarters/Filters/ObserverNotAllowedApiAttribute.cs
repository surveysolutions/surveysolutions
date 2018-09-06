using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;

namespace WB.UI.Headquarters.Filters
{
    public class ObserverNotAllowedApiAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            IAuthorizedUser authorizedUser  = filterContext.Request.GetDependencyScope().GetService(typeof(IAuthorizedUser)) as IAuthorizedUser;

            if (authorizedUser.IsObserver)
            {
                filterContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)
                {
                    Content = new StringContent(Strings.ObserverNotAllowed)
                };
            }
            else
                base.OnActionExecuting(filterContext);
        }
    }
}
