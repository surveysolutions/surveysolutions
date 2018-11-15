using System.Web.Mvc;

namespace WB.UI.Shared.Web.Modules.Filters
{
    public class MvcExceptionFilter<TFilter> : System.Web.Mvc.IExceptionFilter
        where TFilter : System.Web.Mvc.IExceptionFilter
    {
        private readonly TFilter filter;

        public MvcExceptionFilter(TFilter filter)
        {
            this.filter = filter;
        }

        public void OnException(ExceptionContext filterContext)
        {
            filter.OnException(filterContext);
        }
    }
}
