﻿using System;
using Ninject;
using Ninject.Modules;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Sql;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.TabletInformation;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Implementation.TemporaryDataStorage;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement
{
    using WB.Core.SharedKernels.SurveyManagement.Views.User;

    public class SurveyManagementSharedKernelModule : NinjectModule
    {
        private readonly string currentFolderPath;
        private readonly int supportedQuestionnaireVersionMajor;
        private readonly int supportedQuestionnaireVersionMinor;
        private readonly int supportedQuestionnaireVersionPatch;
        private readonly Func<bool> isDebug;
        private readonly InterviewDetailsDataLoaderSettings interviewDetailsDataLoaderSettings;
        private readonly Version applicationBuildVersion;
        private readonly bool hqEnabled;
        private readonly int maxCountOfCachedEntitiesForSqliteDb;
        private readonly InterviewHistorySettings interviewHistorySettings;

        public SurveyManagementSharedKernelModule(string currentFolderPath,
            int supportedQuestionnaireVersionMajor, int supportedQuestionnaireVersionMinor,
            int supportedQuestionnaireVersionPatch,
            Func<bool> isDebug, Version applicationBuildVersion,
            InterviewDetailsDataLoaderSettings interviewDetailsDataLoaderSettings, bool hqEnabled, int maxCountOfCachedEntitiesForSqliteDb,
            InterviewHistorySettings interviewHistorySettings)
        {
            this.currentFolderPath = currentFolderPath;
            this.supportedQuestionnaireVersionMajor = supportedQuestionnaireVersionMajor;
            this.supportedQuestionnaireVersionMinor = supportedQuestionnaireVersionMinor;
            this.supportedQuestionnaireVersionPatch = supportedQuestionnaireVersionPatch;
            this.isDebug = isDebug;
            this.interviewDetailsDataLoaderSettings = interviewDetailsDataLoaderSettings;
            this.applicationBuildVersion = applicationBuildVersion;
            this.hqEnabled = hqEnabled;
            this.maxCountOfCachedEntitiesForSqliteDb = maxCountOfCachedEntitiesForSqliteDb;
            this.interviewHistorySettings = interviewHistorySettings;
        }

        public override void Load()
        {
            //this.Bind<IUserViewFactory>().To<UserViewFactory>(); // binded automatically but should not

            this.Bind<ISampleImportService>().To<SampleImportService>();
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
            
            var applicationVersionSettings = new ApplicationVersionSettings
            {
                SupportedQuestionnaireVersionMajor = this.supportedQuestionnaireVersionMajor,
                SupportedQuestionnaireVersionMinor = this.supportedQuestionnaireVersionMinor,
                SupportedQuestionnaireVersionPatch = this.supportedQuestionnaireVersionPatch
            };
            this.Unbind<ISupportedVersionProvider>();
            this.Bind<ISupportedVersionProvider>()
                .ToConstant(new SupportedVersionProvider(applicationVersionSettings, this.isDebug, this.applicationBuildVersion));

            this.Bind<ISyncProtocolVersionProvider>().To<SyncProtocolVersionProvider>().InSingletonScope();

            this.Bind(typeof (ITemporaryDataStorage<>)).To(typeof (FileTemporaryDataStorage<>));

            this.Bind<IQuestionnaireCacheInitializer>().To<QuestionnaireCacheInitializer>();
            this.Bind<InterviewDetailsDataLoaderSettings>().ToConstant(this.interviewDetailsDataLoaderSettings);
            this.Bind<InterviewDetailsBackgroundSchedulerTask>().ToSelf();

            this.Bind<ITabletInformationService>().To<FileBasedTabletInformationService>().WithConstructorArgument("parentFolder", this.currentFolderPath);
            this.Bind<IDataExportWriter>().To<SqlDataExportWriter>();
            this.Bind<ISqlDataAccessor>().To<SqlDataAccessor>();
            this.Bind<ISqlServiceFactory>().To<SqliteServiceFactory>();

            this.Bind<IEnvironmentContentService>().To<StataEnvironmentContentService>();
            this.Bind<IExportViewFactory>().To<ExportViewFactory>();
            this.Bind<IReferenceInfoForLinkedQuestionsFactory>().To<ReferenceInfoForLinkedQuestionsFactory>();

            this.Bind<IPasswordHasher>().To<PasswordHasher>().InSingletonScope(); // external class which cannot be put to self-describing module because ninject is not portable

            this.Kernel.RegisterDenormalizer<InterviewEventHandlerFunctional>();
            this.Kernel.RegisterDenormalizer<SynchronizationDenormalizer>();
            if (hqEnabled)
            {
                this.Kernel.RegisterDenormalizer<InterviewExportedDataDenormalizer>();
                this.Kernel.RegisterDenormalizer<QuestionnaireExportStructureDenormalizer>();
            }
            this.Bind<IUnhandledPackageStorage>()
                .To<UnhandledPackageStorage>();

            this.Bind<ISyncPackagesProcessor>()
                .To<SyncPackagesProcessor>()
                .InSingletonScope();

            this.Bind<IIncomingSyncPackagesQueue>()
              .To<IncomingSyncPackagesQueue>()
              .InSingletonScope();

            this.Bind<IFolderPermissionChecker>().To<FolderPermissionChecker>().WithConstructorArgument("folderPath", this.currentFolderPath); ;
            
            this.Bind<InterviewHistorySettings>().ToConstant(interviewHistorySettings);
            
            this.Bind<IInterviewHistoryFactory>().To<InterviewHistoryFactory>();

            if (interviewHistorySettings.EnableInterviewHistory)
            {
                this.Unbind<IReadSideRepositoryWriter<InterviewHistoryView>>();
                this.Bind<IReadSideRepositoryWriter<InterviewHistoryView>>().To<InterviewHistoryWriter>().InSingletonScope();
                this.Kernel.RegisterDenormalizer<InterviewHistoryDenormalizer>();
            }
        }
    }
}
