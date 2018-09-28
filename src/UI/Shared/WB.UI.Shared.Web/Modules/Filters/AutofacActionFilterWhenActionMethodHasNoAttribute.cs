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
    public class AutofacActionFilterWhenActionMethodHasNoAttribute<TFilter, TAttribute> : IAutofacActionFilter
        where TFilter : System.Web.Http.Filters.ActionFilterAttribute
        where TAttribute : Attribute
    {
        public AutofacActionFilterWhenActionMethodHasNoAttribute(TFilter filter)
        {
            this.filter = filter;
        }

        private bool shouldExecute = false;
        private readonly TFilter filter;

        public Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var attributes = actionContext.ActionDescriptor.GetCustomAttributes<TAttribute>();
            shouldExecute = attributes == null || attributes.Count == 0;

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
    }
}
