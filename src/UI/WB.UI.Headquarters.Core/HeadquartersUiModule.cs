using System;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using reCAPTCHA.AspNetCore;
using WB.Core.BoundedContexts.Headquarters.PdfInterview;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Enumerator.Native.WebInterview.Pipeline;
using WB.Enumerator.Native.WebInterview.Services;
using WB.UI.Headquarters.Code.Authentication;
using WB.UI.Headquarters.Code.WebInterview.Pipeline;
using WB.UI.Headquarters.Configs;
using WB.UI.Headquarters.Controllers.Api.PublicApi;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Models;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.Api;
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

            registry.Bind<IInterviewerProfileFactory, InterviewerProfileFactory>();
            registry.Bind<IImageProcessingService, ImageProcessingService>();
            registry.Bind<IApplicationRestarter, ApplicationRestarter>();
            registry.BindAsSingleton<IAudioProcessingService, AudioProcessingService>();
            services.Bind<ICaptchaService, WebCacheBasedCaptchaService>();
            registry.Bind<IWebInterviewInterviewEntityFactory, HqWebInterviewInterviewEntityFactory>();
            registry.Bind<IWebNavigationService, WebNavigationService>();
            registry.Bind<IReviewAllowedService, ReviewAllowedService>();
            registry.Bind<IQuestionnaireAssemblyAccessor, QuestionnaireAssemblyAccessor>();
            registry.Bind<IViewRenderService, ViewRenderService>();

            registry.Bind<IWebInterviewNotificationService, WebInterviewNotificationService>();

            registry.Bind<IPipelineModule, PauseResumePipelineModule>();
            registry.Bind<UpdateRequiredFilter>();
            registry.Bind<IPdfInterviewGenerator, PdfInterviewGenerator>();
            registry.Bind<IWebInterviewTimezoneSetter, WebInterviewTimezoneSetter>();

            registry.BindToConstant<IMapper>(_ => new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new WebInterviewAutoMapProfile());
                cfg.AddProfile(new AssignmentProfile());
                cfg.AddProfile(new AssignmentsPublicApiMapProfile());
                cfg.AddProfile(new WorkspacePublicApiMapProfile());
                cfg.ConstructServicesUsing(_.Get);
            }).CreateMapper());

            var captchaSection = this.configuration.CaptchaOptionsSection();

            ConfigureEventBus(registry);

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

        private void ConfigureEventBus(IIocRegistry registry)
        {
            EventHandlersConfig eventBusConfig = configuration.GetSection("EventHandlers").Get<EventHandlersConfig>();

            var eventBusSettings = new EventBusSettings
            {
                DisabledEventHandlerTypes =
                    eventBusConfig.Disabled
                        .Select(Type.GetType)
                        .Where(x => x != null)
                        .ToArray(),

                EventHandlerTypesWithIgnoredExceptions =
                    eventBusConfig.IgnoredException
                        .Select(Type.GetType)
                        .Where(x => x != null)
                        .ToArray(),
            };

            registry.BindToConstant(() => eventBusSettings);
        }
    }
}
