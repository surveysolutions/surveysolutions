using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using WB.Services.Export.Events;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Host.Infra
{
    public class TenantInfoPropagationActionFilter : IActionFilter
    {
        private readonly IServiceProvider provider;

        public TenantInfoPropagationActionFilter(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var tenant = context.ActionArguments.Values.OfType<TenantInfo>().FirstOrDefault() 
                ?? context.HttpContext.Request.GetTenantInfo();
            if(tenant != null)
            {
                provider.SetTenant(tenant);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
