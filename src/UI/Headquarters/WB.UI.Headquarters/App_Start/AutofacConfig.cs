using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using System.Web.Http;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.Storage;
using WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.BoundedContexts.Headquarters.UserPreloading;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native;
using WB.Infrastructure.Native.Files;
using WB.Infrastructure.Native.Logging;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Persistence.Headquarters.Migrations.Events;
using WB.Persistence.Headquarters.Migrations.PlainStore;
using WB.Persistence.Headquarters.Migrations.ReadSide;
using WB.Persistence.Headquarters.Migrations.Users;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Injections;
using WB.UI.Shared.Web.Captcha;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.Extensions;
using WB.UI.Shared.Web.Kernel;
using WB.UI.Shared.Web.Modules;
using WB.UI.Shared.Web.Settings;
using WB.UI.Shared.Web.Versions;


namespace WB.UI.Headquarters
{
    [Localizable(false)]
    public static class AutofacConfig
    {
        public static AutofacWebKernel CreateKernel()
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

            var applicationSecuritySection = settingsProvider.GetSection<HqSecuritySection>(@"applicationSecurity");

            Database.SetInitializer(new FluentMigratorInitializer<HQIdentityDbContext>("users", DbUpgradeSettings.FromFirstMigration<M001_AddUsersHqIdentityModel>()));

            UnitOfWorkConnectionSettings connectionSettings = new UnitOfWorkConnectionSettings
            {
                ConnectionString = settingsProvider.ConnectionStrings[dbConnectionStringName].ConnectionString,
                ReadSideMappingAssemblies = mappingAssemblies,
                PlainStorageSchemaName = "plainstore",
                PlainMappingAssemblies = new List<Assembly>
                {
                    typeof(HeadquartersBoundedContextModule).Assembly,
                    typeof(ProductVersionModule).Assembly,
                },
                PlainStoreUpgradeSettings = new DbUpgradeSettings(typeof(M001_Init).Assembly, typeof(M001_Init).Namespace),
                ReadSideUpgradeSettings = new DbUpgradeSettings(typeof(M001_Init).Assembly, typeof(M001_InitDb).Namespace)
            };
            
            var eventStoreSettings = new PostgreConnectionSettings
            {
                ConnectionString = settingsProvider.ConnectionStrings[dbConnectionStringName].ConnectionString,
                SchemaName = "events"
            };

            var eventStoreModule = new PostgresWriteSideModule(eventStoreSettings,
                new DbUpgradeSettings(typeof(M001_AddEventSequenceIndex).Assembly, typeof(M001_AddEventSequenceIndex).Namespace));

            var autofacKernel = new AutofacWebKernel();

            var interviewCountLimitString = settingsProvider.AppSettings["Limits.MaxNumberOfInterviews"];
            int? interviewCountLimit = string.IsNullOrEmpty(interviewCountLimitString)
                ? (int?) null
                : int.Parse(interviewCountLimitString);

            var userPreloadingConfigurationSection = 
                settingsProvider.GetSection<UserPreloadingConfigurationSection>("userPreloadingSettingsGroup/userPreloadingSettings") ??
                new UserPreloadingConfigurationSection();

            var userPreloadingSettings =
                new UserPreloadingSettings(
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
                    settingsProvider.AppSettings["Export.ServiceUrl"],
                    settingsProvider.AppSettings["Export.MaxRecordsCountPerOneExportQuery"].ToIntOrDefault(10000),
                    settingsProvider.AppSettings["Export.LimitOfCachedItemsByDenormalizer"].ToIntOrDefault(100),
                    settingsProvider.AppSettings["Export.InterviewsExportParallelTasksLimit"].ToIntOrDefault(10),
                    settingsProvider.AppSettings["Export.InterviewIdsQueryBatchSize"].ToIntOrDefault(40000),
                    settingsProvider.AppSettings["Export.ErrorsExporterBatchSize"].ToIntOrDefault(20)
                    );

            var sampleImportSettings = new SampleImportSettings(
                settingsProvider.AppSettings["PreLoading.InterviewsImportParallelTasksLimit"].ToIntOrDefault(2));

            ExternalStoragesSettings externalStoragesSettings = new FakeExternalStoragesSettings();

            if (settingsProvider.TryGetSection<ExternalStoragesConfigSection>("externalStorages", out var externalStoragesSection))
            {
                externalStoragesSettings = new ExternalStoragesSettings
                {
                    OAuth2 = new ExternalStoragesSettings.OAuth2Settings
                    {
                        RedirectUri = externalStoragesSection.OAuth2.RedirectUri,
                        ResponseType = externalStoragesSection.OAuth2.ResponseType,
                        OneDrive = new ExternalStoragesSettings.ExternalStorageOAuth2Settings
                        {
                            ClientId = externalStoragesSection.OAuth2.OneDrive.ClientId,
                            AuthorizationUri = externalStoragesSection.OAuth2.OneDrive.AuthorizationUri,
                            Scope = externalStoragesSection.OAuth2.OneDrive.Scope
                        },
                        Dropbox = new ExternalStoragesSettings.ExternalStorageOAuth2Settings
                        {
                            ClientId = externalStoragesSection.OAuth2.Dropbox.ClientId,
                            AuthorizationUri = externalStoragesSection.OAuth2.Dropbox.AuthorizationUri,
                            Scope = externalStoragesSection.OAuth2.Dropbox.Scope
                        },
                        GoogleDrive = new ExternalStoragesSettings.ExternalStorageOAuth2Settings
                        {
                            ClientId = externalStoragesSection.OAuth2.GoogleDrive.ClientId,
                            AuthorizationUri = externalStoragesSection.OAuth2.GoogleDrive.AuthorizationUri,
                            Scope = externalStoragesSection.OAuth2.GoogleDrive.Scope
                        },
                    }
                };
            }

            //for assembly relocation during migration
            var legacyAssemblySettings = new LegacyAssemblySettings()
            {
                FolderPath = basePath,
                AssembliesDirectoryName = "QuestionnaireAssemblies"
            };

            var trackingSettings = GetTrackingSettings(settingsProvider);

            var owinSecurityModule = new OwinSecurityModule();
            
            autofacKernel.Load(new NLogLoggingModule(),
                
                new OrmModule(connectionSettings),

                new InfrastructureModule(),
                new NcqrsModule(),
                new WebConfigurationModule(),
                new CaptchaModule(settingsProvider.AppSettings.Get("CaptchaService")),
                new QuestionnaireUpgraderModule(),
                new FileInfrastructureModule(),
                
                new ProductVersionModule(typeof(HeadquartersUIModule).Assembly),

                eventStoreModule,
                new DataCollectionSharedKernelModule(),
                new FileStorageModule(basePath),
                new QuartzModule(),
                new WebInterviewModule(),
                new HqWebInterviewModule(),
                owinSecurityModule,

                new HeadquartersBoundedContextModule(basePath,
                    interviewDetailsDataLoaderSettings,
                    userPreloadingSettings,
                    exportSettings,
                    interviewDataExportSettings,
                    sampleImportSettings,
                    synchronizationSettings,
                    trackingSettings,
                    interviewCountLimit,
                    externalStoragesSettings: externalStoragesSettings
                    )

                );

            autofacKernel.Load(new HeadquartersUIModule(),
                               new MainModule(settingsProvider, applicationSecuritySection, legacyAssemblySettings));

            
/*

            autofacKernel.ContainerBuilder.RegisterAssemblyTypes(typeof(UpgradeAssignmentJob).Assembly)
                .Where(type => !type.IsAbstract && typeof(IJob).IsAssignableFrom(type))
                .AsSelf().InstancePerLifetimeScope();
*/

            return autofacKernel;
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
    }



    /*public class CustomHubActivator : IHubActivator
    {
        private readonly IKernel _container;

        public CustomHubActivator(IKernel container)
        {
            _container = container;
        }

        public IHub Create(HubDescriptor descriptor)
        {
            return _container.Get(descriptor.HubType) as IHub;
        }
    }*/
}
