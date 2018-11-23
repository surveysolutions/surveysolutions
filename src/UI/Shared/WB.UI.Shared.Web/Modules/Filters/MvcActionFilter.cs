using System.Web.Mvc;

namespace WB.UI.Shared.Web.Modules.Filters
{
    public class MvcActionFilter : ActionFilterAttribute
    {
        private readonly ActionFilterAttribute filterInstance;

        public MvcActionFilter(ActionFilterAttribute filter)
        {
            this.filterInstance = filter;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterInstance.OnActionExecuting(filterContext);

            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            filterInstance.OnActionExecuted(filterContext);
            base.OnActionExecuted(filterContext);
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            filterInstance.OnResultExecuting(filterContext);
            base.OnResultExecuting(filterContext);
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            filterInstance.OnResultExecuted(filterContext);
            base.OnResultExecuted(filterContext);
        }
    }
}
