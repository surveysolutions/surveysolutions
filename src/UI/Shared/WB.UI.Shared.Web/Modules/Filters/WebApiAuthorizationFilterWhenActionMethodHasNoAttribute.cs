using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Autofac.Integration.WebApi;

namespace WB.UI.Shared.Web.Modules.Filters
{
    public class WebApiAuthorizationFilterWhenActionMethodHasNoAttribute<TFilter, TAttribute> : IAutofacAuthorizationFilter
        where TFilter : System.Web.Http.Filters.AuthorizationFilterAttribute
        where TAttribute : Attribute
    {
        public WebApiAuthorizationFilterWhenActionMethodHasNoAttribute(TFilter filter)
        {
            this.filter = filter;
        }

        private readonly TFilter filter;

        public Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var attributes = actionContext.ActionDescriptor.GetCustomAttributes<TAttribute>();
            bool shouldExecute = attributes == null || attributes.Count == 0;

            if (shouldExecute)
            {
                return filter.OnAuthorizationAsync(actionContext, cancellationToken);
            }

            return Task.CompletedTask;
        }
    }
}