using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Shared.Web.UnderConstruction
{
    public static class UnderConstructionExtensions
    {
        public static IServiceCollection AddUnderConstruction(this IServiceCollection services, 
            UnderConstructionInfo underConstructionInfo)
        {
            services.AddSingleton(underConstructionInfo);
            return services;
        }

        public static IServiceCollection AddUnderConstruction(this IServiceCollection services)
        {
            services.AddUnderConstruction(new UnderConstructionInfo());
            return services;
        }

        public static IApplicationBuilder UseUnderConstruction(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UnderConstructionMiddleware>();
        }
    }
}
