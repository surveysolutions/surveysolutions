using Microsoft.AspNetCore.Mvc.Filters;

namespace WB.UI.Shared.Web.Modules.Filters
{
    public class MvcActionFilter<TFilter> : IActionFilter, IResultFilter
        where TFilter : ActionFilterAttribute
    {
        private readonly TFilter filterInstance;

        public MvcActionFilter(TFilter filter)
        {
            this.filterInstance = filter;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterInstance.OnActionExecuting(filterContext);
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            filterInstance.OnActionExecuted(filterContext);
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            filterInstance.OnResultExecuting(filterContext);
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
            filterInstance.OnResultExecuted(filterContext);
        }
    }
}
