using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using reCAPTCHA.AspNetCore;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Configs;
using WB.Core.BoundedContexts.Headquarters.Maps;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Infrastructure.Native;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Configs;
using WB.UI.Shared.Web.Captcha;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters
{
    public static class AppConfiguration
    {
        public static void AddOptionsConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IOptions<HeadquartersConfig>>(sp =>
            {
                var workspaceAccessor = sp.GetRequiredService<IWorkspaceContextAccessor>();
                var config = configuration.HeadquarterOptions().Get<HeadquartersConfig>();
                config.BaseAppUrl = config.BaseUrl;
                var workspace = workspaceAccessor.CurrentWorkspace();

                if (workspace != null)
                {
                    config.BaseUrl = config.BaseAppUrl + "/" + workspace.Name;
                }

                config.BaseUrl = config.BaseUrl.TrimEnd('/');

                return Options.Create(config);
            });
            
            // configuration
            services.Configure<ApkConfig>(configuration.GetSection("Apks"));
            services.Configure<CaptchaConfig>(configuration.CaptchaOptionsSection());
            services.Configure<ExportServiceConfig>(configuration.GetSection("DataExport"));
            services.Configure<DesignerConfig>(configuration.GetSection("Designer"));
            services.Configure<GoogleMapsConfig>(configuration.GetSection("GoogleMap"));
            
            services.Configure<PreloadingConfig>(configuration.GetSection("PreLoading"));
            services.Configure<RecaptchaSettings>(configuration.CaptchaOptionsSection());
            services.Configure<SchedulerConfig>(configuration.GetSection("Scheduler"));
           
            services.Configure<GeospatialConfig>(configuration.GetSection("Geospatial"));
            
            services.Configure<MetricsConfig>(configuration.MetricsConfiguration());

            services.Configure<VersionCheckConfig>(configuration.GetSection("VersionCheck"));
        }

    public static IConfigurationSection MetricsConfiguration(this IConfiguration conf)
    {
        return conf.GetSection("Metrics");
    }
}
}
