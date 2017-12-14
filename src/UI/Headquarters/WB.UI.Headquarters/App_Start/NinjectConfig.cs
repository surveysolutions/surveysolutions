using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Mvc;
using AutoMapper;
using Main.DenormalizerStorage;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Ninject;
using Microsoft.Owin.Security;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Common;
using Ninject.Web.WebApi.FilterBindingSyntax;
using Quartz;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.DataExport.Jobs;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Maps;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.BoundedContexts.Headquarters.UserPreloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.BoundedContexts.Headquarters.WebInterview.Jobs;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.EventBus.Lite.Implementation;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Binding;
using WB.Infrastructure.Native;
using WB.Infrastructure.Native.Files;
using WB.Infrastructure.Native.Logging;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation.Migrations;
using WB.UI.Headquarters.API.Attributes;
using WB.UI.Headquarters.API.PublicApi;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Implementation.Maps;
using WB.UI.Headquarters.Implementation.Services;
using WB.UI.Headquarters.Injections;
using WB.UI.Headquarters.Migrations.PlainStore;
using WB.UI.Headquarters.Migrations.ReadSide;
using WB.UI.Headquarters.Migrations.Users;
using WB.UI.Headquarters.Models.Api;
using WB.UI.Headquarters.Models.WebInterview;
using WB.UI.Headquarters.Services;
using WB.UI.Shared.Web.Captcha;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.Extensions;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.MembershipProvider.Accounts;
using WB.UI.Shared.Web.Modules;
using WB.UI.Shared.Web.Settings;
using WB.UI.Shared.Web.Versions;
using FilterScope = System.Web.Http.Filters.FilterScope;

namespace WB.UI.Headquarters
{
    [Localizable(false)]
    public static class NinjectConfig
    {
        public static IKernel CreateKernel()
        {
            var settingsProvider = new SettingsProvider();
            
            var useBackgroundJobForProcessingPackages = settingsProvider.AppSettings.GetBool("Synchronization.UseBackgroundJobForProcessingPackages", @default: false);
            var interviewDetailsDataLoaderSettings =
                new SyncPackagesProcessorBackgroundJobSetting(useBackgroundJobForProcessingPackages,
                    ApplicationSettings.InterviewDetailsDataSchedulerSynchronizationInterval,
                    synchronizationBatchCount:
                    settingsProvider.AppSettings.GetInt("Scheduler.SynchronizationBatchCount", @default: 5),
                    synchronizationParallelExecutorsCount:
                    settingsProvider.AppSettings.GetInt("Scheduler.SynchronizationParallelExecutorsCount",
                        @default: 1));

            string appDataDirectory = settingsProvider.AppSettings["DataStorePath"];
            if (appDataDirectory.StartsWith("~/") || appDataDirectory.StartsWith(@"~\"))
            {
                appDataDirectory = HostingEnvironment.MapPath(appDataDirectory);
            }

            var synchronizationSettings = new SyncSettings(origin: Constants.SupervisorSynchronizationOrigin,
                useBackgroundJobForProcessingPackages: useBackgroundJobForProcessingPackages);

            var basePath = appDataDirectory;
            
            var mappingAssemblies = new List<Assembly> {typeof(HeadquartersBoundedContextModule).Assembly};

            var dbConnectionStringName = @"Postgres";

            var postgresPlainStorageSettings = new PostgresPlainStorageSettings()
            {
                ConnectionString = settingsProvider.ConnectionStrings[dbConnectionStringName].ConnectionString,
                SchemaName = "plainstore",
                DbUpgradeSettings = new DbUpgradeSettings(typeof(M001_Init).Assembly, typeof(M001_Init).Namespace),
                MappingAssemblies = new List<Assembly>
                {
                    typeof(HeadquartersBoundedContextModule).Assembly,
                    typeof(ProductVersionModule).Assembly,
                }
            };

            var cacheSettings = new ReadSideCacheSettings(cacheSizeInEntities: settingsProvider.AppSettings.GetInt("ReadSide.CacheSize", @default: 1024),
                storeOperationBulkSize: settingsProvider.AppSettings.GetInt("ReadSide.BulkSize", @default: 512));

            var applicationSecuritySection = settingsProvider.GetSection<HqSecuritySection>(@"applicationSecurity");

            Database.SetInitializer(new FluentMigratorInitializer<HQIdentityDbContext>("users", DbUpgradeSettings.FromFirstMigration<M001_AddUsersHqIdentityModel>()));

            var kernel = new StandardKernel(
                new NinjectSettings {InjectNonPublic = true},
                new NLogLoggingModule().AsNinject(),
                new ServiceLocationModule(),
                new EventSourcedInfrastructureModule().AsNinject(),
                new InfrastructureModule().AsNinject(),
                new NcqrsModule().AsNinject(),
                new WebConfigurationModule().AsNinject(),
                new CaptchaModule(settingsProvider.AppSettings.Get("CaptchaService")).AsNinject(),
                new QuestionnaireUpgraderModule().AsNinject(),
                new FileInfrastructureModule().AsNinject(),
                new ProductVersionModule(typeof(HeadquartersUIModule).Assembly).AsNinject(),
                new HeadquartersUIModule().AsWebNinject(),
                new PostgresKeyValueModule(cacheSettings).AsNinject(),
                new PostgresReadSideModule(
                    settingsProvider.ConnectionStrings[dbConnectionStringName].ConnectionString,
                    PostgresReadSideModule.ReadSideSchemaName, DbUpgradeSettings.FromFirstMigration<M001_InitDb>(),
                    cacheSettings,
                    mappingAssemblies).AsNinject()
            );
            
            kernel.Bind<IEventSourcedAggregateRootRepository, IAggregateRootCacheCleaner>().To<EventSourcedAggregateRootRepositoryWithWebCache>().InSingletonScope();

            var eventStoreSettings = new PostgreConnectionSettings
            {
                ConnectionString = settingsProvider.ConnectionStrings[dbConnectionStringName].ConnectionString,
                SchemaName = "events"
            };

            var eventStoreModule = new PostgresWriteSideModule(eventStoreSettings,
                new DbUpgradeSettings(typeof(M001_AddEventSequenceIndex).Assembly, typeof(M001_AddEventSequenceIndex).Namespace)).AsNinject();

            var interviewCountLimitString = settingsProvider.AppSettings["Limits.MaxNumberOfInterviews"];
            int? interviewCountLimit = string.IsNullOrEmpty(interviewCountLimitString)
                ? (int?) null
                : int.Parse(interviewCountLimitString);

            var userPreloadingConfigurationSection = 
                settingsProvider.GetSection<UserPreloadingConfigurationSection>("userPreloadingSettingsGroup/userPreloadingSettings") ??
                new UserPreloadingConfigurationSection();

            var userPreloadingSettings =
                new UserPreloadingSettings(
                    userPreloadingConfigurationSection.ExecutionIntervalInSeconds,
                    userPreloadingConfigurationSection.MaxAllowedRecordNumber,
                    loginFormatRegex: UserModel.UserNameRegularExpression,
                    emailFormatRegex: userPreloadingConfigurationSection.EmailFormatRegex,
                    passwordFormatRegex: applicationSecuritySection.PasswordPolicy.PasswordStrengthRegularExpression,
                    phoneNumberFormatRegex: userPreloadingConfigurationSection.PhoneNumberFormatRegex,
                    fullNameMaxLength: UserModel.PersonNameMaxLength,
                    phoneNumberMaxLength: UserModel.PhoneNumberLength,
                    personNameFormatRegex: UserModel.PersonNameRegex);

            var exportSettings = new ExportSettings(
                settingsProvider.AppSettings["Export.BackgroundExportIntervalInSeconds"].ToIntOrDefault(15));
            var interviewDataExportSettings =
                new InterviewDataExportSettings(basePath,
                    bool.Parse(settingsProvider.AppSettings["Export.EnableInterviewHistory"]),
                    settingsProvider.AppSettings["Export.MaxRecordsCountPerOneExportQuery"].ToIntOrDefault(10000),
                    settingsProvider.AppSettings["Export.LimitOfCachedItemsByDenormalizer"].ToIntOrDefault(100),
                    settingsProvider.AppSettings["Export.InterviewsExportParallelTasksLimit"].ToIntOrDefault(10),
                    settingsProvider.AppSettings["Export.InterviewIdsQueryBatchSize"].ToIntOrDefault(40000),
                    settingsProvider.AppSettings["Export.ErrorsExporterBatchSize"].ToIntOrDefault(20));

            var sampleImportSettings = new SampleImportSettings(
                settingsProvider.AppSettings["PreLoading.InterviewsImportParallelTasksLimit"].ToIntOrDefault(2));

            //for assembly relocation during migration
            kernel.Bind<LegacyAssemblySettings>().ToConstant(new LegacyAssemblySettings()
            {
                FolderPath = basePath,
                AssembliesDirectoryName = "QuestionnaireAssemblies"
            });


            var trackingSettings = GetTrackingSettings(settingsProvider);

            var owinSecurityModule = new OwinSecurityModule();


            kernel.Load(
                new PostgresPlainStorageModule(postgresPlainStorageSettings).AsNinject(),
                eventStoreModule,
                new DataCollectionSharedKernelModule().AsNinject(),
                new HeadquartersBoundedContextModule(basePath,
                    interviewDetailsDataLoaderSettings,
                    userPreloadingSettings,
                    exportSettings,
                    interviewDataExportSettings,
                    sampleImportSettings,
                    synchronizationSettings,
                    trackingSettings,
                    interviewCountLimit).AsNinject(),
                new QuartzModule().AsNinject(),
                new WebInterviewModule().AsNinject(),
                owinSecurityModule.AsNinject());

            kernel.Bind<ILiteEventRegistry>().To<LiteEventRegistry>();
            kernel.Bind<ISettingsProvider>().ToConstant(settingsProvider);

            ModelBinders.Binders.DefaultBinder = new GenericBinderResolver(kernel);

            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
            CreateAndRegisterEventBus(kernel);

            kernel.Bind<ITokenVerifier>()
                .ToConstant(new SimpleTokenVerifier(settingsProvider.AppSettings["Synchronization.Key"]));

            kernel.BindHttpFilter<TokenValidationAuthorizationFilter>(FilterScope.Controller)
                .WhenControllerHas<TokenValidationAuthorizationAttribute>();

            kernel.BindHttpFilter<TokenValidationAuthorizationFilter>(FilterScope.Controller)
                .WhenControllerHas<ApiValidationAntiForgeryTokenAttribute>()
                .WithConstructorArgument("tokenVerifier", new ApiValidationAntiForgeryTokenVerifier());

            kernel.Bind(typeof(InMemoryReadSideRepositoryAccessor<>)).ToSelf().InSingletonScope();

            ServiceLocator.Current.GetInstance<InterviewDetailsBackgroundSchedulerTask>().Configure();
            ServiceLocator.Current.GetInstance<UsersImportTask>().Configure();
            ServiceLocator.Current.GetInstance<ExportJobScheduler>().Configure();
            ServiceLocator.Current.GetInstance<PauseResumeJobScheduler>().Configure();

            ServiceLocator.Current.GetInstance<IScheduler>().Start();
            
            kernel.Unbind<IInterviewImportService>();
            kernel.Bind<IInterviewImportService>().To<InterviewImportService>().InSingletonScope();

            var autoMapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new WebInterviewAutoMapProfile());
                cfg.AddProfile(new AssignmentProfile());
                cfg.AddProfile(new AssignmentsPublicApiMapProfile());
                cfg.ConstructServicesUsing(t => kernel.Get(t));
            });
            kernel.Bind<IMapper>().ToConstant(autoMapperConfig.CreateMapper());
            kernel.Bind<JsonSerializer>().ToConstant(JsonSerializer.Create(new JsonSerializerSettings
            {
                ContractResolver = new FilteredCamelCasePropertyNamesContractResolver
                {
                    AssembliesToInclude =
                    {
                        typeof(WebInterview).Assembly,
                        typeof(CategoricalOption).Assembly
                    }
                }
            }));
            
            kernel.Bind<IPasswordPolicy>().ToConstant(new PasswordPolicy
            {
                PasswordMinimumLength = applicationSecuritySection.PasswordPolicy.PasswordMinimumLength,
                MinRequiredNonAlphanumericCharacters = applicationSecuritySection.PasswordPolicy.MinRequiredNonAlphanumericCharacters,
                PasswordStrengthRegularExpression = applicationSecuritySection.PasswordPolicy.PasswordStrengthRegularExpression
            });

            kernel.Bind<GoogleApiSettings>()
                .ToMethod(_ => new GoogleApiSettings(_.Kernel.Get<IConfigurationManager>().AppSettings[@"Google.Map.ApiKey"]))
                .InSingletonScope();

            kernel.Bind<IInterviewCreatorFromAssignment>().To<InterviewCreatorFromAssignment>();

            kernel.Bind<IMapPropertiesProvider>().To<MapPropertiesProvider>();

            owinSecurityModule.Init(ServiceLocator.Current);

            GlobalHost.DependencyResolver = new NinjectDependencyResolver(kernel);

            return kernel;
        }

        private static TrackingSettings GetTrackingSettings(SettingsProvider settingsProvider)
        {
            if (TimeSpan.TryParse(settingsProvider.AppSettings["Tracking.WebInterviewPauseResumeGraceTimespan"],
                CultureInfo.InvariantCulture, out TimeSpan timespan))
            {
                return new TrackingSettings(timespan);
            }
            return new TrackingSettings(TimeSpan.FromMinutes(2));
        }

        private static void CreateAndRegisterEventBus(StandardKernel kernel)
        {
            var eventBusConfigSection = kernel.Get<ISettingsProvider>().GetSection<EventBusConfigSection>("eventBus");

            var bus = new NcqrCompatibleEventDispatcher(
                kernel.Get<IEventStore>(),
                eventBusConfigSection.GetSettings(),
                kernel.Get<ILogger>());

            kernel.Bind<IEventBus>().ToConstant(bus);
            kernel.Bind<ILiteEventBus>().ToConstant(bus);
            kernel.Bind<IEventDispatcher>().ToConstant(bus);

            //Kernel.RegisterDenormalizer<>() - should be used instead
            var enumerable = kernel.GetAll(typeof(IEventHandler)).ToList();
            foreach (object handler in enumerable)
            {
                bus.Register((IEventHandler)handler);
            }
        }
    }
}