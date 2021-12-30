using System;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Headquarters.HealthChecks
{
    public static class HqHealthChecks
    {
        public static void AddHeadquartersHealthCheck(this IServiceCollection services)
        {
            services.AddHostedService<AmazonS3CheckService>();

            var checks = services.AddHealthChecks();

            if (!Environment.GetEnvironmentVariable("NO_HQ_BASEURL_CHECK").ToBool(false))
            {
                checks.AddCheck<HeadquartersUrlBindingCheck>("hq_baseurl_check");
            }

            checks
                .AddCheck<HeadquartersStartupCheck>("under_construction_check", tags: new[] { "ready" })
                .AddCheck<ExportServiceVersionCheck>("export_service_check")
                .AddCheck<ExportServiceConnectivityCheck>("export_service_connectivity_check")
                //.AddCheck<BrokenPackagesCheck>("broken_packages_check")
                .AddCheck<DatabaseConnectionCheck>("database_connection_check");
                
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                checks.AddCheck<AspTempDirectoryAccessCheck>("temp folder permissions");
        }
    }
}
