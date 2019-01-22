using System;
using System.Web.Mvc;

namespace WB.UI.Shared.Web.Modules.Filters
{
    public class MvcActionFilterWhenControllerOrActionHasNoAttribute<TFilter, TAttribute> : IActionFilter, IResultFilter
        where TFilter : System.Web.Mvc.ActionFilterAttribute
        where TAttribute : Attribute
    {
        private readonly TFilter filterInstance;
        private bool shouldExecute;

        public MvcActionFilterWhenControllerOrActionHasNoAttribute(TFilter filterInstance)
        {
            this.filterInstance = filterInstance;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            shouldExecute = ShouldExecute(filterContext.ActionDescriptor, filterContext.Controller);

            if (shouldExecute)
            {
                filterInstance.OnActionExecuting(filterContext);
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (shouldExecute)
            {
                filterInstance.OnActionExecuted(filterContext);
            }
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (shouldExecute)
            {
                filterInstance.OnResultExecuting(filterContext);
            }
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (shouldExecute)
            {
                filterInstance.OnResultExecuted(filterContext);
            }
        }

        private bool ShouldExecute(ActionDescriptor actionDescriptor, ControllerBase baseController)
        {
            var actionAttributes = actionDescriptor.GetCustomAttributes(typeof(TAttribute), true);
            var controllerAttributes = baseController.GetType().GetCustomAttributes(typeof(TAttribute), true);
            bool shouldExecute = (actionAttributes == null || actionAttributes.Length == 0)
                                 && (controllerAttributes == null || controllerAttributes.Length == 0);
            return shouldExecute;
        }
    }
}
