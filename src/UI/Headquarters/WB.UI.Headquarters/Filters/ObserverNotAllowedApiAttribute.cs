using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Filters
{
    public class ObserverNotAllowedApiAttribute : ActionFilterAttribute
    {
        private IIdentityManager identityManager => ServiceLocator.Current.GetInstance<IIdentityManager>();

        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            if (this.identityManager.IsCurrentUserObserver)
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