using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using System.Web.Routing;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.UI.Shared.Web.Controllers;
using WB.UI.Shared.Web.Resources;
using WB.UI.Shared.Web.Settings;

namespace WB.UI.Shared.Web.Filters
{
    public static class HttpActionContextExtensions
    {
        public static T Resolve<T>(this HttpActionContext actionContext) where T : class
        {
            var requestScope = actionContext.Request.GetDependencyScope();
            return requestScope.GetService(typeof(T)) as T;
        }
    }

    public class UnderConstructionHttpFilter : System.Web.Http.Filters.ActionFilterAttribute
    {
        public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var status = actionContext.Resolve<UnderConstructionInfo>();

            if (status.Status != UnderConstructionStatus.Finished)
            {
                if (CoreSettings.IsDevelopmentEnvironment)
                {
                    status.Completed.Wait(cancellationToken);
                }
                else
                {
                    actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new StringContent(status.Message ?? UnderConstruction.ServerInitializing),
                    };
                }
                return Task.CompletedTask;
            }

            return base.OnActionExecutingAsync(actionContext, cancellationToken);
        }
    }

    public class UnderConstructionMvcFilter : System.Web.Mvc.ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.Controller.GetType() != typeof(UnderConstructionController))
            {
                var status = DependencyResolver.Current.GetService<UnderConstructionInfo>();

                if (status.Status != UnderConstructionStatus.Finished)
                {
                    if (CoreSettings.IsDevelopmentEnvironment)
                        status.Completed.Wait();
                    else
                        filterContext.Result = new RedirectToRouteResult("", new RouteValueDictionary(new {controller = "UnderConstruction", action = "Index"}));
                    return;
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
