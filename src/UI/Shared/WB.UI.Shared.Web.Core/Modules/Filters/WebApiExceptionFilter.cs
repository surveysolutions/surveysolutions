using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Autofac.Integration.WebApi;

namespace WB.UI.Shared.Web.Modules.Filters
{
    public class WebApiExceptionFilter<TFilter> : IAutofacExceptionFilter where TFilter : ExceptionFilterAttribute
    {
        private readonly TFilter filter;

        public WebApiExceptionFilter(TFilter filter)
        {
            this.filter = filter;
        }

        public Task OnExceptionAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            return filter.OnExceptionAsync(actionExecutedContext, cancellationToken);
        }
    }
}
