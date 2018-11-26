using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Autofac.Integration.WebApi;

namespace WB.UI.Shared.Web.Modules.Filters
{
    public class WebApiAuthorizationFilter<TFilter> : IAutofacAuthorizationFilter where TFilter : AuthorizationFilterAttribute
    {
        private readonly TFilter filter;

        public WebApiAuthorizationFilter(TFilter filter)
        {
            this.filter = filter;
        }

        public Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            return filter.OnAuthorizationAsync(actionContext, cancellationToken);
        }
    }
}