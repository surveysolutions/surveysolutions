using Microsoft.Extensions.DependencyInjection;

namespace WB.UI.Headquarters.HealthChecks
{
    public static class HqHealthChecks
    {
        public static void AddHeadquartersHealthCheck(this IServiceCollection services)
        {
            services.AddHostedService<AmazonS3CheckService>();
            
            services.AddHealthChecks()
                .AddCheck<HeadquartersUrlBindingCheck>("hq_baseurl_check")
                .AddCheck<HeadquartersStartupCheck>("under_construction_check", tags: new[] { "ready" })
                .AddCheck<ExportServiceVersionCheck>("export_service_check")
                .AddCheck<ExportServiceConnectivityCheck>("export_service_connectivity_check")
                .AddCheck<BrokenPackagesCheck>("broken_packages_check")
                .AddCheck<DatabaseConnectionCheck>("database_connection_check");
        }
    }
}
