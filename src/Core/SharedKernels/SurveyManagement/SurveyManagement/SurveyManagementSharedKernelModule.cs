using System;
using Ninject;
using Ninject.Modules;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.TabletInformation;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization.IncomePackagesRepository;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.SharedKernels.SurveyManagement.Implementation.TemporaryDataStorage;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement
{
    public class SurveyManagementSharedKernelModule : NinjectModule
    {
        private readonly string currentFolderPath;
        private readonly int supportedQuestionnaireVersionMajor;
        private readonly int supportedQuestionnaireVersionMinor;
        private readonly int supportedQuestionnaireVersionPatch;
        private readonly Func<bool> isDebug;
        private readonly InterviewDetailsDataLoaderSettings interviewDetailsDataLoaderSettings;
        private readonly Version applicationBuildVersion;

        public SurveyManagementSharedKernelModule(string currentFolderPath,
            int supportedQuestionnaireVersionMajor, int supportedQuestionnaireVersionMinor, int supportedQuestionnaireVersionPatch,
            Func<bool> isDebug, Version applicationBuildVersion,
            InterviewDetailsDataLoaderSettings interviewDetailsDataLoaderSettings)
        {
            this.currentFolderPath = currentFolderPath;
            this.supportedQuestionnaireVersionMajor = supportedQuestionnaireVersionMajor;
            this.supportedQuestionnaireVersionMinor = supportedQuestionnaireVersionMinor;
            this.supportedQuestionnaireVersionPatch = supportedQuestionnaireVersionPatch;
            this.isDebug = isDebug;
            this.interviewDetailsDataLoaderSettings = interviewDetailsDataLoaderSettings;
            this.applicationBuildVersion = applicationBuildVersion;
        }

        public override void Load()
        {
            this.Bind<ISampleImportService>().To<SampleImportService>();
            this.Bind<IDataExportService>().To<FileBasedDataExportService>().WithConstructorArgument("folderPath", this.currentFolderPath);
            this.Bind<IPreloadingTemplateService>().To<PreloadingTemplateService>().WithConstructorArgument("folderPath", this.currentFolderPath);
            this.Bind<IPreloadedDataRepository>().To<FilebasedPreloadedDataRepository>().WithConstructorArgument("folderPath", this.currentFolderPath);
            this.Bind<IQuestionnareAssemblyFileAccessor>().To<QuestionnaireAssemblyFileAccessor>().WithConstructorArgument("folderPath", this.currentFolderPath);
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

            this.Bind(typeof (ITemporaryDataStorage<>)).To(typeof (FileTemporaryDataStorage<>));

            Action<Guid> additionalEventChecker = this.AdditionalEventChecker;


            this.Bind<IQuestionnaireCacheInitializer>().To<QuestionnaireCacheInitializer>();
            this.Bind<IReadSideRepositoryReader<InterviewData>>()
                .To<ReadSideRepositoryReaderWithSequence<InterviewData>>().InSingletonScope()
                .WithConstructorArgument("additionalEventChecker", additionalEventChecker);

            this.Bind<IInterviewDetailsDataLoader>().To<InterviewDetailsDataLoader>();
            this.Bind<IInterviewDetailsDataProcessor>().To<InterviewDetailsDataProcessor>();
            this.Bind<InterviewDetailsDataProcessorContext>().ToSelf().InSingletonScope();
            this.Bind<InterviewDetailsDataLoaderSettings>().ToConstant(this.interviewDetailsDataLoaderSettings);
            this.Bind<InterviewDetailsBackgroundSchedulerTask>().ToSelf();

            this.Bind<ITabletInformationService>().To<FileBasedTabletInformationService>().WithConstructorArgument("parentFolder", this.currentFolderPath);
            this.Bind<IDataFileExportService>().To<CsvDataFileExportService>();
            this.Bind<IEnvironmentContentService>().To<StataEnvironmentContentService>();
            this.Bind<IExportViewFactory>().To<ExportViewFactory>();
            this.Bind<IReferenceInfoForLinkedQuestionsFactory>().To<ReferenceInfoForLinkedQuestionsFactory>();

            this.Bind<IPasswordHasher>().To<PasswordHasher>().InSingletonScope(); // external class which cannot be put to self-describing module because ninject is not portable


            this.Bind<IIncomePackagesRepository>().To<IncomePackagesRepository>().InSingletonScope();
            this.Bind<IInterviewExpressionStateProvider>().To<WB.Core.SharedKernels.SurveyManagement.Implementation.QuestionnaireAssembly.InterviewExpressionStateProvider>().InSingletonScope();
            
            //this.Bind<IChartStatisticsViewFactory>().To<ChartStatisticsViewFactory>();
        }

        protected void AdditionalEventChecker(Guid interviewId)
        {
            this.Kernel.Get<IIncomePackagesRepository>().ProcessItem(interviewId);
        }
    }
}
