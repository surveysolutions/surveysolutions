﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Autofac.Integration.WebApi;

namespace WB.UI.Shared.Web.Modules.Filters
{
    public class WebApiAuthorizationFilterWhenControllerOrActionHasNoAttribute<TFilter, TAttribute> : IAutofacAuthorizationFilter
        where TFilter : System.Web.Http.Filters.AuthorizationFilterAttribute
        where TAttribute : Attribute
    {
        public WebApiAuthorizationFilterWhenControllerOrActionHasNoAttribute(TFilter filter)
        {
            this.filter = filter;
        }

        private readonly TFilter filter;

        public Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var actionAttributes = actionContext.ActionDescriptor.GetCustomAttributes<TAttribute>();
            var controllerAttributes = actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<TAttribute>();
            bool shouldExecute = (actionAttributes == null || actionAttributes.Count == 0)
                                 && (controllerAttributes == null || controllerAttributes.Count == 0);

            if (shouldExecute)
            {
                return filter.OnAuthorizationAsync(actionContext, cancellationToken);
            }

            return Task.CompletedTask;
        }
    }
}
