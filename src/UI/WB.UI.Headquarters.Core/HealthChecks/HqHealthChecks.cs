using Microsoft.Extensions.DependencyInjection;

namespace WB.UI.Headquarters.HealthChecks
{
    public static class HqHealthChecks
    {
        public static void AddHeadquartersHealthCheck(this IServiceCollection services)
        {
            services.AddHostedService<AmazonS3CheckService>();
            
            services.AddHealthChecks()
                .AddCheck<HeadquartersStartupCheck>("under_construction_check", tags: new[] { "ready" })
                .AddCheck<ExportServiceCheck>("export_service_check")
                .AddCheck<BrokenPackagesCheck>("broken_packages_check")
                .AddCheck<DatabaseConnectionCheck>("database_connection_check");
        }
    }
}
