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
    public class WebApiAuthorizationFilterWhenActionMethodHasAttribute<TFilter, TAttribute> : IAutofacAuthorizationFilter
        where TFilter : System.Web.Http.Filters.IAuthorizationFilter
        where TAttribute : Attribute
    {
        public WebApiAuthorizationFilterWhenActionMethodHasAttribute(TFilter filter)
        {
            this.filter = filter;
        }

        private readonly TFilter filter;

        public Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var actionAttributes = actionContext.ActionDescriptor.GetCustomAttributes<TAttribute>();
            var controllerAttributes = actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<TAttribute>();
            bool shouldExecute = (actionAttributes != null && actionAttributes.Count > 0)
                            || (controllerAttributes != null && controllerAttributes.Count > 0);

            if (shouldExecute)
            {
                return filter.ExecuteAuthorizationFilterAsync(actionContext, cancellationToken, () => null);
            }

            return Task.CompletedTask;
        }
    }


    public class WebApiAuthorizationFilterWhenActionMethodHasAttribute: IAuthorizationFilter
    {
        public WebApiAuthorizationFilterWhenActionMethodHasAttribute(IAuthorizationFilter filter, Type attributeType)
        {
            this.filter = filter;
            this.attributeType = attributeType;
        }

        private readonly IAuthorizationFilter filter;
        private readonly Type attributeType;

        public Task<HttpResponseMessage> ExecuteAuthorizationFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken,
            Func<Task<HttpResponseMessage>> continuation)
        {
            var shouldExecute = FilterExtensions.HasActionOrControllerMarkerAttribute(actionContext.ActionDescriptor, attributeType);

            if (shouldExecute)
            {
                return filter.ExecuteAuthorizationFilterAsync(actionContext, cancellationToken, continuation);
            }

            return continuation();
        }

        public bool AllowMultiple => true;
    }
}
