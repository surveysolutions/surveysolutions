using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Shared.Web.UnderConstruction
{
    public static class UnderConstructionExtensions
    {
        public static IServiceCollection AddUnderConstruction(this IServiceCollection services)
        {
            return services;
        }

        public static IApplicationBuilder UseUnderConstruction(this IApplicationBuilder builder)
        {
            return builder.UseWhen(UnderConstructionMiddleware.When,                
                app => app.UseMiddleware<UnderConstructionMiddleware>());
        }
    }
}
