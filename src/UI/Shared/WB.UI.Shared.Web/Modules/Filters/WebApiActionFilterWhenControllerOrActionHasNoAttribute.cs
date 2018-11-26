using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Autofac.Integration.WebApi;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.UI.Shared.Web.Modules.Filters
{
    public class WebApiActionFilterWhenControllerOrActionHasNoAttribute<TFilter, TAttribute> : IAutofacActionFilter
        where TFilter : System.Web.Http.Filters.ActionFilterAttribute
        where TAttribute : Attribute
    {
        public WebApiActionFilterWhenControllerOrActionHasNoAttribute(TFilter filter)
        {
            this.filter = filter;
        }

        private bool shouldExecute = false;
        private readonly TFilter filter;

        public Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var actionAttributes = actionContext.ActionDescriptor.GetCustomAttributes<TAttribute>();
            var controllerAttributes = actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<TAttribute>();
            shouldExecute = (actionAttributes == null || actionAttributes.Count == 0)
                            && (controllerAttributes == null || controllerAttributes.Count == 0);

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

        public bool AllowMultiple => true;
    }

    public class WebApiActionFilterWhenControllerOrActionHasNoAttribute : ActionFilterAttribute
    {
        public WebApiActionFilterWhenControllerOrActionHasNoAttribute(ActionFilterAttribute filter, Type attributeType)
        {
            this.attributeType = attributeType;
            this.filter = filter;
        }

        private readonly Type attributeType;
        private readonly ActionFilterAttribute filter;

        public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var shouldExecute = !FilterExtensions.HasActionOrControllerMarkerAttribute(actionContext.ActionDescriptor, attributeType);

            if (shouldExecute)
            {
                return filter.OnActionExecutingAsync(actionContext, cancellationToken);
            }

            return Task.CompletedTask;
        }

        public override Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            var shouldExecute = !FilterExtensions.HasActionOrControllerMarkerAttribute(actionExecutedContext.ActionContext.ActionDescriptor, attributeType);
            if (shouldExecute)
            {
                return filter.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
            }

            return Task.CompletedTask;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var shouldExecute = !FilterExtensions.HasActionOrControllerMarkerAttribute(actionContext.ActionDescriptor, attributeType);
            if (shouldExecute)
            {
                filter.OnActionExecuting(actionContext);
            }

            base.OnActionExecuting(actionContext);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var shouldExecute = !FilterExtensions.HasActionOrControllerMarkerAttribute(actionExecutedContext.ActionContext.ActionDescriptor, attributeType);
            if (shouldExecute)
            {
                filter.OnActionExecuted(actionExecutedContext);
            }

            base.OnActionExecuted(actionExecutedContext);
        }
    }
}
