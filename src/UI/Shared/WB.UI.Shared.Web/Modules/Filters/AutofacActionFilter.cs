using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Autofac.Integration.WebApi;

namespace WB.UI.Shared.Web.Modules.Filters
{
    public class AutofacActionFilter<TFilter> : IAutofacActionFilter where TFilter : ActionFilterAttribute
    {
        private readonly TFilter filter;

        public AutofacActionFilter(TFilter filter)
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
}
