using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Autofac.Integration.WebApi;

namespace WB.UI.Shared.Web.Modules.Filters
{
    public class WebApiExceptionFilterWhenActionMethodHasNoAttribute<TFilter, TAttribute> : IAutofacExceptionFilter
        where TFilter : System.Web.Http.Filters.ExceptionFilterAttribute
        where TAttribute : Attribute
    {
        private readonly TFilter filter;

        public WebApiExceptionFilterWhenActionMethodHasNoAttribute(TFilter filter)
        {
            this.filter = filter;
        }

        public Task OnExceptionAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            var attributes = actionExecutedContext.ActionContext.ActionDescriptor.GetCustomAttributes<TAttribute>();
            bool shouldExecute = attributes == null || attributes.Count == 0;

            if (shouldExecute)
            {
                return filter.OnExceptionAsync(actionExecutedContext, cancellationToken);
            }

            return Task.CompletedTask;
        }
    }
}