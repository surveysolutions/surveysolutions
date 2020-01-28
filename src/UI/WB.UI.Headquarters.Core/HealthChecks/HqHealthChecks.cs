using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WB.UI.Headquarters.HealthChecks
{
    public static class HqHealthChecks
    {
        public static void AddHeadquartersHealthCheck(this IServiceCollection services)
        {
            services.AddHostedService<AmazonS3CheckService>();
        }
    }
}
