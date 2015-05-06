using System;
using Ninject;
using Ninject.Modules;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DeleteQuestionnaireTemplate;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck.Checks;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Sql;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.TabletInformation;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Implementation.TemporaryDataStorage;
using WB.Core.SharedKernels.SurveyManagement.QuartzIntegration;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.DeleteQuestionnaireTemplate;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Services.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Core.Synchronization;
using WB.Core.Synchronization.EventHandler;

namespace WB.Core.SharedKernels.SurveyManagement
{
    public class SurveyManagementSharedKernelModule : NinjectModule
    {
        private readonly string currentFolderPath;
        private readonly Func<bool> isDebug;
        private readonly InterviewDetailsDataLoaderSettings interviewDetailsDataLoaderSettings;
        private readonly Version applicationBuildVersion;
        private readonly bool hqEnabled;
        private readonly bool isSupervisorFunctionsEnabled;
        private readonly int maxCountOfCachedEntitiesForSqliteDb;
        private readonly InterviewHistorySettings interviewHistorySettings;

        public SurveyManagementSharedKernelModule(string currentFolderPath,
            Func<bool> isDebug, Version applicationBuildVersion,
            InterviewDetailsDataLoaderSettings interviewDetailsDataLoaderSettings, bool hqEnabled, int maxCountOfCachedEntitiesForSqliteDb,
            InterviewHistorySettings interviewHistorySettings,
            bool isSupervisorFunctionsEnabled)
        {
            this.currentFolderPath = currentFolderPath;
            this.isDebug = isDebug;
            this.interviewDetailsDataLoaderSettings = interviewDetailsDataLoaderSettings;
            this.applicationBuildVersion = applicationBuildVersion;
            this.hqEnabled = hqEnabled;
            this.maxCountOfCachedEntitiesForSqliteDb = maxCountOfCachedEntitiesForSqliteDb;
            this.interviewHistorySettings = interviewHistorySettings;
            this.isSupervisorFunctionsEnabled = isSupervisorFunctionsEnabled;
        }

        public override void Load()
        {
            //this.Bind<IUserViewFactory>().To<UserViewFactory>(); // binded automatically but should not

            this.Bind<ISampleImportService>().To<SampleImportService>();
            this.Bind<Func<ISampleImportService>>().ToMethod(context => () => context.Kernel.Get<ISampleImportService>());
            this.Bind<IFilebasedExportedDataAccessor>().To<FilebasedExportedDataAccessor>().WithConstructorArgument("folderPath", this.currentFolderPath);
            this.Bind<IDataExportService>().To<SqlToTabDataExportService>();
            this.Bind<FileBasedDataExportRepositorySettings>().ToConstant(new FileBasedDataExportRepositorySettings(maxCountOfCachedEntitiesForSqliteDb));
            this.Bind<IDataExportRepositoryWriter>().To<FileBasedDataExportRepositoryWriter>().InSingletonScope();
            this.Bind<IPreloadingTemplateService>().To<PreloadingTemplateService>().WithConstructorArgument("folderPath", this.currentFolderPath);
            this.Bind<IPreloadedDataRepository>().To<FilebasedPreloadedDataRepository>().WithConstructorArgument("folderPath", this.currentFolderPath);
            this.Bind<IPreloadedDataVerifier>().To<PreloadedDataVerifier>();
            this.Bind<IRecordsAccessorFactory>().To<CsvRecordsAccessorFactory>();
            this.Bind<ICsvWriterService>().To<CsvWriterService>();
            this.Bind<ICsvWriterFactory>().To<CsvWriterFactory>();
            this.Bind<IQuestionDataParser>().To<QuestionDataParser>();
            this.Bind<IPreloadedDataService>().To<PreloadedDataService>();
            this.Bind<IInterviewSynchronizationDtoFactory>().To<InterviewSynchronizationDtoFactory>();
            this.Bind<IPreloadedDataServiceFactory>().To<PreloadedDataServiceFactory>();

            this.Bind<ISupervisorTeamsAndStatusesReport>().To<SupervisorTeamsAndStatusesReport>();
            this.Bind<IHeadquartersTeamsAndStatusesReport>().To<HeadquartersTeamsAndStatusesReport>();
            this.Bind<ISurveysAndStatusesReport>().To<SurveysAndStatusesReport>();

            this.Unbind<ISupportedVersionProvider>();
            this.Bind<ISupportedVersionProvider>()
                .ToConstant(new SupportedVersionProvider(this.isDebug, this.applicationBuildVersion));

            this.Bind<ISyncProtocolVersionProvider>().To<SyncProtocolVersionProvider>().InSingletonScope();

            this.Bind(typeof(ITemporaryDataStorage<>)).To(typeof(FileTemporaryDataStorage<>)).WithConstructorArgument("rootPath", this.currentFolderPath);

            this.Bind<IQuestionnaireCacheInitializer>().To<QuestionnaireCacheInitializer>();
            this.Bind<InterviewDetailsDataLoaderSettings>().ToConstant(this.interviewDetailsDataLoaderSettings);
            this.Bind<InterviewDetailsBackgroundSchedulerTask>().ToSelf();

            this.Bind<ITabletInformationService>().To<FileBasedTabletInformationService>().WithConstructorArgument("parentFolder", this.currentFolderPath);
            this.Bind<IDataExportWriter>().To<ReadSideRepositoryDataExportWriter>();
            this.Bind<ISqlDataAccessor>().To<SqlDataAccessor>();

            this.Bind<IEnvironmentContentService>().To<StataEnvironmentContentService>();
            this.Bind<IExportViewFactory>().To<ExportViewFactory>();
            this.Bind<IReferenceInfoForLinkedQuestionsFactory>().To<ReferenceInfoForLinkedQuestionsFactory>();

            this.Bind<IPasswordHasher>().To<PasswordHasher>().InSingletonScope(); // external class which cannot be put to self-describing module because ninject is not portable

            this.Bind(typeof(IOrderableSyncPackageWriter<,>)).To(typeof(OrderableSyncPackageWriter<,>)).InSingletonScope();

            this.Kernel.RegisterDenormalizer<InterviewEventHandlerFunctional>();
            this.Kernel.RegisterDenormalizer<StatusChangeHistoryDenormalizerFunctional>();

            this.Kernel.Load(new QuartzNinjectModule());

            if (isSupervisorFunctionsEnabled)
            {
                this.Kernel.RegisterDenormalizer<InterviewSynchronizationDenormalizer>();
                this.Kernel.RegisterDenormalizer<UserSynchronizationDenormalizer>();
                this.Kernel.RegisterDenormalizer<QuestionnaireSynchronizationDenormalizer>();
                this.Kernel.RegisterDenormalizer<TabletDenormalizer>();
            }

            if (hqEnabled)
            {
                this.Kernel.RegisterDenormalizer<InterviewExportedDataDenormalizer>();
                this.Kernel.RegisterDenormalizer<QuestionnaireExportStructureDenormalizer>();
            }
            this.Bind<IBrokenSyncPackagesStorage>()
                .To<BrokenSyncPackagesStorage>();

            this.Bind<ISyncPackagesProcessor>()
                .To<SyncPackagesProcessor>()
                .InSingletonScope();

            this.Bind<IIncomingSyncPackagesQueue>()
              .To<IncomingSyncPackagesQueue>()
              .InSingletonScope();

            this.Bind<ReadSideService>().ToSelf().InSingletonScope();
            this.Bind<IReadSideStatusService>().ToMethod(context => context.Kernel.Get<ReadSideService>());
            this.Bind<IReadSideAdministrationService>().ToMethod(context => context.Kernel.Get<ReadSideService>());

            this.Bind<IInterviewsToDeleteFactory>().To<InterviewsToDeleteFactory>();
            this.Bind<Func<IInterviewsToDeleteFactory>>().ToMethod(context => () => context.Kernel.Get<IInterviewsToDeleteFactory>());
            this.Bind<IDeleteQuestionnaireService>().To<DeleteQuestionnaireService>().InSingletonScope();

            this.Bind<InterviewHistorySettings>().ToConstant(interviewHistorySettings);

            this.Bind<IInterviewHistoryFactory>().To<InterviewHistoryFactory>();

            if (interviewHistorySettings.EnableInterviewHistory)
            {
                this.Unbind<IReadSideRepositoryWriter<InterviewHistoryView>>();
                this.Bind<IReadSideRepositoryWriter<InterviewHistoryView>>().To<InterviewHistoryWriter>().InSingletonScope();
                this.Kernel.RegisterDenormalizer<InterviewHistoryDenormalizer>();
            }

            this.Bind<IAtomicHealthCheck<EventStoreHealthCheckResult>>().To<EventStoreHealthCheck>();
            this.Bind<IAtomicHealthCheck<FolderPermissionCheckResult>>().To<FolderPermissionChecker>().WithConstructorArgument("folderPath", this.currentFolderPath);
            this.Bind<IAtomicHealthCheck<NumberOfSyncPackagesWithBigSizeCheckResult>>().To<NumberOfSyncPackagesWithBigSizeChecker>();
            this.Bind<IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult>>().To<NumberOfUnhandledPackagesChecker>();
            this.Bind<IHealthCheckService>().To<HealthCheckService>();
        }
    }
}
