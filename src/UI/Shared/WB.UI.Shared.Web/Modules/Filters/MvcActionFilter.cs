using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Mvc;
using Autofac.Integration.WebApi;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using ActionFilterAttribute = System.Web.Mvc.ActionFilterAttribute;

namespace WB.UI.Shared.Web.Modules.Filters
{
    public class MvcActionFilter : ActionFilterAttribute
    {
        private readonly Type filter;
        private ActionFilterAttribute filterInstance;

        public MvcActionFilter(Type filter)
        {
            this.filter = filter;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterInstance = (ActionFilterAttribute)ServiceLocator.Current.GetInstance(filter);
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
