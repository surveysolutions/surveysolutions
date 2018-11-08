using System.Threading.Tasks;
using AutoMapper;
using Newtonsoft.Json;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Maps;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Jobs;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks;
using WB.Core.BoundedContexts.Headquarters.WebInterview.Jobs;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.EventBus.Lite.Implementation;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Enumerator.Native.JsonConversion;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Infrastructure.Native;
using WB.Infrastructure.Native.Monitoring;
using WB.Infrastructure.Native.Storage;
using WB.UI.Headquarters.API.Attributes;
using WB.UI.Headquarters.API.PublicApi;
using WB.UI.Headquarters.Code;
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
    public class MainModule : IWebModule
    {
        private readonly SettingsProvider settingsProvider;
        private readonly HqSecuritySection applicationSecuritySection;
        private readonly LegacyAssemblySettings legacyAssemblySettings;
        
        public MainModule(SettingsProvider settingsProvider, HqSecuritySection applicationSecuritySection, LegacyAssemblySettings legacyAssemblySettings)
        {
            this.settingsProvider = settingsProvider;
            this.applicationSecuritySection = applicationSecuritySection;
            this.legacyAssemblySettings = legacyAssemblySettings;
        }

        public void Load(IWebIocRegistry registry)
        {
            registry.Bind<ILiteEventRegistry, LiteEventRegistry>();
            registry.BindToConstant<ISettingsProvider>(() => settingsProvider);

            registry.BindToConstant<LegacyAssemblySettings>(() => legacyAssemblySettings);

            registry.BindWebApiAuthorizationFilter<CustomWebApiAuthorizeFilter>();

            registry.BindToConstant<ITokenVerifier>(() => new SimpleTokenVerifier(settingsProvider.AppSettings["Synchronization.Key"]));

            registry.BindWebApiAuthorizationFilterWhenControllerOrActionHasAttribute<TokenValidationAuthorizationFilter, TokenValidationAuthorizationAttribute>();

            registry.BindWebApiAuthorizationFilterWhenControllerOrActionHasAttribute<TokenValidationAuthorizationFilter, ApiValidationAntiForgeryTokenAttribute>(
                new ConstructorArgument("tokenVerifier", _ => new ApiValidationAntiForgeryTokenVerifier()));
            
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

            registry.Bind<IEventSourcedAggregateRootRepository, EventSourcedAggregateRootRepositoryWithWebCache>();
            registry.Bind<IAggregateRootCacheCleaner, EventSourcedAggregateRootRepositoryWithWebCache>();


            EventBusSettings eventBusSettings = settingsProvider.GetSection<EventBusConfigSection>("eventBus").GetSettings();
            registry.BindToConstant(() => eventBusSettings);

            //todo:af
            //could be expensive
            //rethink registration
            registry.BindWithConstructorArgumentInPerLifetimeScope<ILiteEventBus, NcqrCompatibleEventDispatcher>(
                "eventBusSettings",
                eventBusSettings);

            registry.Bind<ISecureStorage, SecureStorage>();
            registry.Bind<IEncryptionService, RsaEncryptionService>();
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            serviceLocator.GetInstance<InterviewDetailsBackgroundSchedulerTask>().Configure();
            serviceLocator.GetInstance<UsersImportTask>().Run();
            serviceLocator.GetInstance<AssignmentsImportTask>().Schedule(repeatIntervalInSeconds: 300);
            serviceLocator.GetInstance<AssignmentsVerificationTask>().Schedule(repeatIntervalInSeconds: 300);
            serviceLocator.GetInstance<DeleteQuestionnaireJobScheduler>().Configure();
            serviceLocator.GetInstance<PauseResumeJobScheduler>().Configure();
            serviceLocator.GetInstance<UpgradeAssignmentJobScheduler>().Configure();

            serviceLocator.GetInstance<IScheduler>().Start();

            InitMetrics();
            MetricsService.Start(serviceLocator);

            return Task.CompletedTask;
        }

        private static void InitMetrics()
        {
            CommonMetrics.StateFullInterviewsCount.Set(0);
        }

    }
}
