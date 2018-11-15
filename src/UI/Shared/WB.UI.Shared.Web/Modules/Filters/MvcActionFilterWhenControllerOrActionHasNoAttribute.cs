using System;
using System.Web.Mvc;

namespace WB.UI.Shared.Web.Modules.Filters
{
    public class MvcActionFilterWhenControllerOrActionHasNoAttribute : ActionFilterAttribute
    {
        private readonly ActionFilterAttribute filterInstance;
        private readonly Type attribute;
        private bool shouldExecute;

        public MvcActionFilterWhenControllerOrActionHasNoAttribute(ActionFilterAttribute filterInstance, Type attribute)
        {
            this.filterInstance = filterInstance;
            this.attribute = attribute;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            shouldExecute = ShouldExecute(filterContext.ActionDescriptor, filterContext.Controller);

            if (shouldExecute)
            {
                filterInstance.OnActionExecuting(filterContext);
            }

            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (shouldExecute)
            {
                filterInstance.OnActionExecuted(filterContext);
            }

            base.OnActionExecuted(filterContext);
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (shouldExecute)
            {
                filterInstance.OnResultExecuting(filterContext);
            }


            base.OnResultExecuting(filterContext);
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (shouldExecute)
            {
                filterInstance.OnResultExecuted(filterContext);
            }

            base.OnResultExecuted(filterContext);
        }


        private bool ShouldExecute(ActionDescriptor actionDescriptor, ControllerBase baseController)
        {
            var actionAttributes = actionDescriptor.GetCustomAttributes(attribute, true);
            var controllerAttributes = baseController.GetType().GetCustomAttributes(attribute, true);
            bool shouldExecute = (actionAttributes == null || actionAttributes.Length == 0)
                                 && (controllerAttributes == null || controllerAttributes.Length == 0);
            return shouldExecute;
        }
    }
}
