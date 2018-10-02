using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Mvc;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.UI.Shared.Web.Attributes;
using ActionFilterAttribute = System.Web.Mvc.ActionFilterAttribute;

namespace WB.UI.Shared.Web.Modules.Filters
{
    public class MvcActionFilterWhenActionMethodHasNoTransactionAttribute : ActionFilterAttribute
    {
        private readonly Type filter;
        private readonly Type attribute;
        private bool shouldExecute;
        private ActionFilterAttribute filterInstance;

        public MvcActionFilterWhenActionMethodHasNoTransactionAttribute(Type TFilter, Type attribute)
        {
            this.filter = TFilter;
            this.attribute = attribute;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            shouldExecute = ShouldExecute(filterContext.ActionDescriptor, filterContext.Controller);

            if (shouldExecute)
            {
                filterInstance = (ActionFilterAttribute)ServiceLocator.Current.GetInstance(filter);
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
