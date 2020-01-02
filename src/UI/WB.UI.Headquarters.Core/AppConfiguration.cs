using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using reCAPTCHA.AspNetCore;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Storage;
using WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Headquarters.Configs;
using WB.UI.Shared.Web.Captcha;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters
{
    public static class AppConfiguration
    {
        public static void AddOptionsConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            // configuration
            services.Configure<AmazonS3Settings>(configuration.AmazonOptions());
            services.Configure<ApkConfig>(configuration.GetSection("Apks"));
            services.Configure<CaptchaConfig>(configuration.CaptchaOptionsSection());
            services.Configure<DataExportOptions>(configuration.GetSection("DataExport"));
            services.Configure<DesignerConfig>(configuration.GetSection("Designer"));
            services.Configure<GoogleMapsConfig>(configuration.GetSection("GoogleMap"));
            services.Configure<HeadquarterOptions>(configuration.HeadquarterOptions());
            services.Configure<PasswordPolicyConfig>(configuration.GetSection("PasswordPolicy"));
            services.Configure<PreloadingConfig>(configuration.GetSection("PreLoading"));
            services.Configure<RecaptchaSettings>(configuration.CaptchaOptionsSection());
            services.Configure<SchedulerConfig>(configuration.GetSection("Scheduler"));
            services.Configure<FileStorageConfig>(configuration.GetSection("FileStorage"));

            services.PostConfigure<FileStorageConfig>(c =>
            {
                c.AppData = c.AppData
                    .Replace("~", System.IO.Directory.GetCurrentDirectory())
                    .Replace("/", @"\\");
            });
        }
    }
}
