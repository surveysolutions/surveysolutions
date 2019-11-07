using Microsoft.AspNetCore.Mvc.Filters;

namespace WB.UI.Shared.Web.Modules.Filters
{
    public class MvcAuthorizationFilter<TFilter> : IAuthorizationFilter
        where TFilter : IAuthorizationFilter
    {
        private readonly TFilter filter;

        public MvcAuthorizationFilter(TFilter filter)
        {
            this.filter = filter;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            filter.OnAuthorization(context);
        }
    }
}
