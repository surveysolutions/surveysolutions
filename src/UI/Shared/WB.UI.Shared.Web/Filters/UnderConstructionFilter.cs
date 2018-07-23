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

namespace WB.UI.Shared.Web.Filters
{
    public class UnderConstructionHttpFilter : System.Web.Http.Filters.ActionFilterAttribute
    {
        public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var status = ServiceLocator.Current.GetInstance<UnderConstructionInfo>();

            if (status.Status != UnderConstructionStatus.Finished)
            {
                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(status.Message ?? UnderConstruction.ServerInitializing),
                };
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
                var status = ServiceLocator.Current.GetInstance<UnderConstructionInfo>();

                if (status.Status != UnderConstructionStatus.Finished)
                {
                    filterContext.Result = new RedirectToRouteResult("", new RouteValueDictionary(new {controller = "UnderConstruction", action = "Index"}));
                    return;
                }
            }

            base.OnActionExecuting(filterContext); 
        }
    }
}
