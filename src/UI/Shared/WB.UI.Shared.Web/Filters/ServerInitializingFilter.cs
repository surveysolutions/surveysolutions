using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Shared.Web.Filters
{
    public class ServerInitializingHttpFilter : System.Web.Http.Filters.ActionFilterAttribute
    {
        public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var status = ServiceLocator.Current.GetInstance<InitModulesStatus>();

            if (status.Status == ServerInitializingStatus.Running)
            {
                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(status.Message ?? "Server Initializing. Please wait..."),
                };
                return Task.CompletedTask;
            }

            return base.OnActionExecutingAsync(actionContext, cancellationToken);
        }
    }

    public class ServerInitializingMvcFilter : System.Web.Mvc.ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var status = ServiceLocator.Current.GetInstance<InitModulesStatus>();

            if (status.Status == ServerInitializingStatus.Running)
            {
                filterContext.Result = new ContentResult()
                {
                    Content = status.Message ?? "Server Initializing. Please wait...",
                    ContentType = "text/html"
                };
                return;
            }

            base.OnActionExecuting(filterContext); 
        }
    }
}
