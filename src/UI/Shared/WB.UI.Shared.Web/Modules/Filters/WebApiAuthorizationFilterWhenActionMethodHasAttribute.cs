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
    public class WebApiAuthorizationFilterWhenActionMethodHasAttribute<TFilter, TAttribute> : IAutofacAuthorizationFilter, IFilter
        where TFilter : System.Web.Http.Filters.IAuthorizationFilter
        where TAttribute : Attribute
    {
        public WebApiAuthorizationFilterWhenActionMethodHasAttribute(TFilter filter)
        {
            this.filter = filter;
        }

        private bool shouldExecute = false;
        private readonly TFilter filter;


        public Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var actionAttributes = actionContext.ActionDescriptor.GetCustomAttributes<TAttribute>();
            var controllerAttributes = actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<TAttribute>();
            shouldExecute = (actionAttributes != null && actionAttributes.Count > 0)
                            || (controllerAttributes != null && controllerAttributes.Count > 0);

            if (shouldExecute)
            {
                return filter.ExecuteAuthorizationFilterAsync(actionContext, cancellationToken, () => null);
            }

            return Task.CompletedTask;
        }

        public Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var actionAttributes = actionContext.ActionDescriptor.GetCustomAttributes<TAttribute>();
            var controllerAttributes = actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<TAttribute>();
            shouldExecute = (actionAttributes != null && actionAttributes.Count > 0)
                            || (controllerAttributes != null && controllerAttributes.Count > 0);

            if (shouldExecute)
            {
                return filter.ExecuteAuthorizationFilterAsync(actionContext, cancellationToken, () => null);
            }

            return Task.CompletedTask;
        }

        public bool AllowMultiple => true;
    }
}
