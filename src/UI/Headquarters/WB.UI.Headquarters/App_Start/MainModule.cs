using AutoMapper;
using Main.DenormalizerStorage;
using Newtonsoft.Json;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.DataExport.Jobs;
using WB.Core.BoundedContexts.Headquarters.Maps;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks;
using WB.Core.BoundedContexts.Headquarters.WebInterview.Jobs;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.EventBus.Lite.Implementation;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection;
using WB.Enumerator.Native.JsonConversion;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Models;
using WB.UI.Headquarters.API.Attributes;
using WB.UI.Headquarters.API.PublicApi;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Implementation.Maps;
using WB.UI.Headquarters.Implementation.Services;
using WB.UI.Headquarters.Models.Api;
using WB.UI.Headquarters.Services;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.MembershipProvider.Accounts;
using WB.UI.Shared.Web.Modules;
using WB.UI.Shared.Web.Settings;
using FilterScope = System.Web.Http.Filters.FilterScope;


namespace WB.UI.Headquarters
{
    public class MainModule : IWebModule, IInitModule
    {
        private readonly SettingsProvider settingsProvider;
        private readonly HqSecuritySection applicationSecuritySection;
        
        public MainModule(SettingsProvider settingsProvider, HqSecuritySection applicationSecuritySection)
        {
            this.settingsProvider = settingsProvider;
            this.applicationSecuritySection = applicationSecuritySection;
        }

        public void Load(IWebIocRegistry registry)
        {
            registry.Bind<ILiteEventRegistry, LiteEventRegistry>();
            registry.BindToConstant<ISettingsProvider>(() => settingsProvider);


            registry.BindToConstant<ITokenVerifier>(() => new SimpleTokenVerifier(settingsProvider.AppSettings["Synchronization.Key"]));

            registry.BindHttpFilterWhenControllerHasAttribute<TokenValidationAuthorizationFilter, TokenValidationAuthorizationAttribute>(FilterScope.Controller);

            registry.BindHttpFilterWhenControllerHasAttribute<TokenValidationAuthorizationFilter, ApiValidationAntiForgeryTokenAttribute>(
                FilterScope.Controller, new ConstructorArgument("tokenVerifier", _ => new ApiValidationAntiForgeryTokenVerifier()));

            registry.BindAsSingleton(typeof(InMemoryReadSideRepositoryAccessor<>), typeof(InMemoryReadSideRepositoryAccessor<>));


            registry.Unbind<IInterviewImportService>();
            registry.BindAsSingleton<IInterviewImportService, InterviewImportService>();

            registry.BindToConstant<IMapper>(_ => new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new WebInterviewAutoMapProfile());
                cfg.AddProfile(new AssignmentProfile());
                cfg.AddProfile(new AssignmentsPublicApiMapProfile());
                cfg.ConstructServicesUsing(_.Get);
            }).CreateMapper());
            registry.BindToConstant(() => JsonSerializer.Create(new JsonSerializerSettings
            {
                ContractResolver = new FilteredCamelCasePropertyNamesContractResolver
                {
                    AssembliesToInclude =
                    {
                        typeof(Startup).Assembly,
                        typeof(WebInterviewModule).Assembly,
                        typeof(CategoricalOption).Assembly
                    }
                }
            }));

            registry.BindToConstant<IPasswordPolicy>(() => new PasswordPolicy
            {
                PasswordMinimumLength = applicationSecuritySection.PasswordPolicy.PasswordMinimumLength,
                MinRequiredNonAlphanumericCharacters = applicationSecuritySection.PasswordPolicy.MinRequiredNonAlphanumericCharacters,
                PasswordStrengthRegularExpression = applicationSecuritySection.PasswordPolicy.PasswordStrengthRegularExpression
            });

            registry.BindToMethodInSingletonScope<GoogleApiSettings>(_ => new GoogleApiSettings(_.Get<IConfigurationManager>().AppSettings[@"Google.Map.ApiKey"]));

            registry.Bind<IInterviewCreatorFromAssignment, InterviewCreatorFromAssignment>();

            registry.Bind<IMapService, MapService>();
        }


        public void Init(IServiceLocator serviceLocator)
        {
            serviceLocator.GetInstance<InterviewDetailsBackgroundSchedulerTask>().Configure();
            serviceLocator.GetInstance<UsersImportTask>().Configure();
            serviceLocator.GetInstance<ExportJobScheduler>().Configure();
            serviceLocator.GetInstance<PauseResumeJobScheduler>().Configure();

            serviceLocator.GetInstance<IScheduler>().Start();
        }
    }
}