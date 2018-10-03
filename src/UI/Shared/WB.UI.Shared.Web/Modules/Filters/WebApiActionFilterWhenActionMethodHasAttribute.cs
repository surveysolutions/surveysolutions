using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Services;
using Autofac.Integration.WebApi;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.UI.Shared.Web.Modules.Filters
{
    public class WebApiActionFilterWhenActionMethodHasAttribute<TFilter, TAttribute> : IAutofacActionFilter
        where TFilter : System.Web.Http.Filters.ActionFilterAttribute
        where TAttribute : Attribute
    {
        public WebApiActionFilterWhenActionMethodHasAttribute(TFilter filter)
        {
            this.filter = filter;
        }

        private bool shouldExecute = false;
        private readonly TFilter filter;

        public Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var actionAttributes = actionContext.ActionDescriptor.GetCustomAttributes<TAttribute>();
            var controllerAttributes = actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<TAttribute>();
            shouldExecute = (actionAttributes != null && actionAttributes.Count > 0)
                            || (controllerAttributes != null && controllerAttributes.Count > 0);

            if (shouldExecute)
            {
                return filter.OnActionExecutingAsync(actionContext, cancellationToken);
            }

            return Task.CompletedTask;
        }

        public Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            if (shouldExecute)
            {
                return filter.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
            }

            return Task.CompletedTask;
        }
    }

    public class WebApiActionFilterWhenActionMethodHasAttribute : ActionFilterAttribute, IAutofacActionFilter
    {
        public WebApiActionFilterWhenActionMethodHasAttribute(ActionFilterAttribute filter, Type attributeType)
        {
            this.filter = filter;
            this.attributeType = attributeType;
        }

        private readonly ActionFilterAttribute filter;
        private readonly Type attributeType;

        public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var shouldExecute = FilterExtensions.HasActionOrControllerMarkerAttribute(actionContext.ActionDescriptor, attributeType);

            if (shouldExecute)
            {
                return filter.OnActionExecutingAsync(actionContext, cancellationToken);
            }

            return Task.CompletedTask;
        }

        public override Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            var shouldExecute = FilterExtensions.HasActionOrControllerMarkerAttribute(actionExecutedContext.ActionContext.ActionDescriptor, attributeType);

            if (shouldExecute)
            {
                return filter.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
            }

            return Task.CompletedTask;
        }
    }
}
