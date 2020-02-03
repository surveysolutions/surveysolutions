using System;
using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using WB.Services.Export.Infrastructure;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Events
{
    public static class TenantContextHelpers
    {
        public static void SetTenant(this IServiceProvider scope, TenantInfo tenant)
        {
            var ctx = scope.GetService<ITenantContext>() as TenantContext;
            if (ctx == null)
            {
                throw new ArgumentException("Cannot get tenant context implementation");
            }

            ctx.Tenant = tenant;
        }
    }
}
