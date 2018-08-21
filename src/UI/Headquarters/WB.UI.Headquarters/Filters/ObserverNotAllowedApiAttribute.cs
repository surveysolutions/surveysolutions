using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.UI.Headquarters.Filters
{
    public class ObserverNotAllowedApiAttribute : ActionFilterAttribute
    {
        private IAuthorizedUser authorizedUser => ServiceLocator.Current.GetInstance<IAuthorizedUser>();

        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            if (this.authorizedUser.IsObserver)
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
