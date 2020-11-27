using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using reCAPTCHA.AspNetCore;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Configs;
using WB.Core.BoundedContexts.Headquarters.Maps;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.SharedKernels.DataCollection;
using WB.Infrastructure.Native;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Configs;
using WB.UI.Shared.Web.Captcha;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters
{
    public static class AppConfiguration
    {
        private static readonly object lockObject = new object();

        public static void AddOptionsConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IOptions<HeadquartersConfig>>(sp =>
            {
                var workspace = sp.GetRequiredService<IWorkspaceContextAccessor>();
                var config = configuration.HeadquarterOptions().Get<HeadquartersConfig>();

                config.BaseUrl = (config.BaseUrl + "/" + workspace.CurrentWorkspace()?.Name ?? string.Empty).TrimEnd('/');

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
            services.Configure<FileStorageConfig>(configuration.GetSection("FileStorage"));
            services.Configure<GeospatialConfig>(configuration.GetSection("Geospatial"));
            
            services.Configure<MetricsConfig>(configuration.MetricsConfiguration());

            services.PostConfigure<FileStorageConfig>(c =>
            {
                c.AppData = c.AppData.Replace("~", Directory.GetCurrentDirectory());
                c.TempData = c.TempData.Replace("~", Directory.GetCurrentDirectory());

                void EnsureFolderExists(string folder)
                {
                    if (Directory.Exists(folder)) return;
                    lock (lockObject)
                    {
                        if (Directory.Exists(folder)) return;

                        Directory.CreateDirectory(folder);
                        Serilog.Log.Information("Created {folder} folder", folder);
                    }
                }

                if (c.GetStorageProviderType() == StorageProviderType.FileSystem)
                {
                    EnsureFolderExists(c.AppData);
                }

                EnsureFolderExists(c.TempData);
            });
        }

        public static IConfigurationSection MetricsConfiguration(this IConfiguration conf)
        {
            return conf.GetSection("Metrics");
        }
    }
}
