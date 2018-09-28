using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Autofac.Integration.WebApi;

namespace WB.UI.Shared.Web.Modules.Filters
{
    public class WebApiAutofacExceptionFilter<TFilter> : IAutofacExceptionFilter where TFilter : ExceptionFilterAttribute
    {
        private readonly TFilter filter;

        public WebApiAutofacExceptionFilter(TFilter filter)
        {
            this.filter = filter;
        }

        public Task OnExceptionAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            return filter.OnExceptionAsync(actionExecutedContext, cancellationToken);
        }
    }

    public class MvcAutofacExceptionFilter<TFilter> : IAutofacExceptionFilter where TFilter : System.Web.Mvc.IExceptionFilter
    {
        private readonly TFilter filter;

        public MvcAutofacExceptionFilter(TFilter filter)
        {
            this.filter = filter;
        }

        public Task OnExceptionAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            return filter.OnException(actionExecutedContext);
        }
    }
}
