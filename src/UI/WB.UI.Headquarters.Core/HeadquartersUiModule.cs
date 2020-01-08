using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using reCAPTCHA.AspNetCore;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.UI.Headquarters.Controllers.Services;
using WB.UI.Headquarters.Services;
using WB.UI.Headquarters.Services.Impl;
using WB.UI.Shared.Web.Captcha;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters
{
    public class HeadquartersUiModule : IModule
    {
        private readonly IConfiguration configuration;

        public HeadquartersUiModule(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void Load(IIocRegistry registry)
        {
            var services = registry;

            services.AddScoped<ServiceApiKeyAuthorization>();
            
            registry.AddTransient<IExportServiceApiFactory, ExportServiceApiFactory>();

            services.AddTransient<ICaptchaService, WebCacheBasedCaptchaService>();

            var captchaSection = this.configuration.CaptchaOptionsSection();


            registry.Bind<IDesignerApiFactory, DesignerApiFactory>();
            registry.BindToMethod(ctx => ctx.Resolve<IDesignerApiFactory>().Get());

            var config = captchaSection.Get<CaptchaConfig>() ?? new CaptchaConfig();
            var provider = config.CaptchaType;

            switch (provider)
            {
                case CaptchaProviderType.Recaptcha:
                    services.AddTransient<IRecaptchaService, RecaptchaService>();
                    services.AddTransient<ICaptchaProvider, RecaptchaProvider>();
                    break;
                case CaptchaProviderType.Hosted:
                    services.AddTransient<ICaptchaProvider, HostedCaptchaProvider>();
                    services.AddTransient<IHostedCaptcha, HostedCaptchaProvider>();
                    break;
                default:
                    services.AddTransient<ICaptchaProvider, NoCaptchaProvider>();
                    break;
            }
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}
