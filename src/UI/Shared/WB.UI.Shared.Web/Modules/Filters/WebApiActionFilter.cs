using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Autofac.Integration.WebApi;

namespace WB.UI.Shared.Web.Modules.Filters
{
    public class WebApiActionFilter<TFilter> : IAutofacActionFilter where TFilter : ActionFilterAttribute
    {
        private readonly TFilter filter;

        public WebApiActionFilter(TFilter filter)
        {
            this.filter = filter;
        }

        public Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            return filter.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
        }

        public Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            return filter.OnActionExecutingAsync(actionContext, cancellationToken);
        }
    }

    public class WebApiActionFilter : ActionFilterAttribute, IAutofacActionFilter
    {
        private readonly ActionFilterAttribute filter;

        public WebApiActionFilter(ActionFilterAttribute filter)
        {
            this.filter = filter;
        }

        public override Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            return filter.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
        }

        public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            return filter.OnActionExecutingAsync(actionContext, cancellationToken);
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            filter.OnActionExecuting(actionContext);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            filter.OnActionExecuted(actionExecutedContext);
        }
    }
}
