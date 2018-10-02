using System.Web.Mvc;

namespace WB.UI.Shared.Web.Modules.Filters
{
    public class MvcAuthorizationFilter<TFilter> : System.Web.Mvc.IAuthorizationFilter
        where TFilter : System.Web.Mvc.IAuthorizationFilter
    {
        private readonly TFilter filter;

        public MvcAuthorizationFilter(TFilter filter)
        {
            this.filter = filter;
        }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            filter.OnAuthorization(filterContext);
        }
    }
}
