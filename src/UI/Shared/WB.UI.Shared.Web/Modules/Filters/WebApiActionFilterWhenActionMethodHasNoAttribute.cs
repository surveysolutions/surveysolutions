using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Autofac.Integration.WebApi;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.UI.Shared.Web.Modules.Filters
{
    public class WebApiActionFilterWhenActionMethodHasNoAttribute<TFilter, TAttribute> : IAutofacActionFilter, IFilter
        where TFilter : System.Web.Http.Filters.ActionFilterAttribute
        where TAttribute : Attribute
    {
        public WebApiActionFilterWhenActionMethodHasNoAttribute(TFilter filter)
        {
            this.filter = filter;
        }

        private bool shouldExecute = false;
        private readonly TFilter filter;

        public Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var actionAttributes = actionContext.ActionDescriptor.GetCustomAttributes<TAttribute>();
            var controllerAttributes = actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<TAttribute>();
            shouldExecute = (actionAttributes == null || actionAttributes.Count == 0)
                            && (controllerAttributes == null || controllerAttributes.Count == 0);

            if (shouldExecute)
            {
                return filter.OnActionExecutingAsync(actionContext, cancellationToken);
            }

            return Task.CompletedTask;
        }

        public Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            if (shouldExecute)
            {
                return filter.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
            }

            return Task.CompletedTask;
        }

        public bool AllowMultiple => true;
    }
}
