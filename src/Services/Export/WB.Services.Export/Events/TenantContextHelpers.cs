using System;
using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using WB.Services.Export.Infrastructure;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Events
{
    public static class TenantContextHelpers
    {
        public static void PropagateTenantContext(this IServiceScope scope, ITenantContext context)
        {
            var ctx = scope.ServiceProvider.GetService<ITenantContext>() as TenantContext;
            if (ctx == null)
            {
                throw new ArgumentException("Cannot get tenant context implementation");
            }

            ctx.Tenant = context.Tenant;
            ctx.Connection = context.Connection;
        }

        
        public static void SetDbConnection(this IServiceScope scope, DbConnection connection)
        {
            var ctx = scope.ServiceProvider.GetService<ITenantContext>() as TenantContext;
            if (ctx == null)
            {
                throw new ArgumentException("Cannot get tenant context implementation");
            }

            ctx.Connection = connection;
        }
        public static void SetTenant(this IServiceProvider scope, TenantInfo tenant)
        {
            var ctx = scope.GetService<ITenantContext>() as TenantContext;
            if (ctx == null)
            {
                throw new ArgumentException("Cannot get tenant context implementation");
            }

            ctx.Tenant = tenant;
        }

        public static string SchemaName(this ITenantContext ctx)
        {
            return ctx.Tenant.Name;
        }
    }
}
