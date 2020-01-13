using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using reCAPTCHA.AspNetCore;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Services;
using WB.UI.Headquarters.API.WebInterview.Services;
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
            
            registry.Bind<IExportServiceApiFactory, ExportServiceApiFactory>();
            registry.Bind<IImageProcessingService, ImageProcessingService>();
            registry.Bind<IAudioProcessingService, AudioProcessingService>();
            services.Bind<ICaptchaService, WebCacheBasedCaptchaService>();
            registry.Bind<IWebInterviewInterviewEntityFactory, WebInterviewInterviewEntityFactory>();
            registry.Bind<IWebNavigationService, WebNavigationService>();
            registry.Bind<IReviewAllowedService, ReviewAllowedService>();
            registry.Bind<IQuestionnaireAssemblyAccessor, QuestionnaireAssemblyAccessor>();
            registry.BindAsSingleton<IWebInterviewNotificationService, WebInterviewNotificationService>();
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
