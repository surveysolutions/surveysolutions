using Ncqrs.Eventing.Storage;
using System;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Common;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.HealthCheck;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.HealthCheck.Checks;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.TabletInformation;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Services.HealthCheck;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.BoundedContexts.Headquarters.UserPreloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.HealthCheck;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Ddi;
using WB.Core.BoundedContexts.Headquarters.DataExport.Ddi.Impl;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Services.Export;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Templates;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Translations;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Translations.Impl;
using WB.Core.BoundedContexts.Headquarters.Services.Internal;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.BoundedContexts.Headquarters.Views.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.Reports;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.BoundedContexts.Headquarters.WebInterview.Impl;

namespace WB.Core.BoundedContexts.Headquarters
{
    public class HeadquartersBoundedContextModule : NinjectModule
    {
        private readonly string currentFolderPath;
        private readonly SyncPackagesProcessorBackgroundJobSetting syncPackagesProcessorBackgroundJobSetting;
        private readonly int? interviewLimitCount;
        private readonly string syncDirectoryName;
        private readonly UserPreloadingSettings userPreloadingSettings;
        private readonly ExportSettings exportSettings;
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly SampleImportSettings sampleImportSettings;
        private readonly SyncSettings syncSettings;

        public HeadquartersBoundedContextModule(string currentFolderPath,
            SyncPackagesProcessorBackgroundJobSetting syncPackagesProcessorBackgroundJobSetting,
            UserPreloadingSettings userPreloadingSettings,
            ExportSettings exportSettings,
            InterviewDataExportSettings interviewDataExportSettings,
            SampleImportSettings sampleImportSettings,
            SyncSettings syncSettings,
            int? interviewLimitCount = null,
            string syncDirectoryName = "SYNC")
        {
            this.userPreloadingSettings = userPreloadingSettings;
            this.exportSettings = exportSettings;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.sampleImportSettings = sampleImportSettings;
            this.currentFolderPath = currentFolderPath;
            this.syncPackagesProcessorBackgroundJobSetting = syncPackagesProcessorBackgroundJobSetting;
            this.interviewLimitCount = interviewLimitCount;
            this.syncSettings = syncSettings;
            this.syncDirectoryName = syncDirectoryName;
        }

        public override void Load()
        {
            this.Bind<IEventTypeResolver>().ToConstant(
                new EventTypeResolver(
                    typeof(DataCollectionSharedKernelAssemblyMarker).Assembly,
                    typeof(HeadquartersBoundedContextModule).Assembly));

            this.Bind<SyncSettings>().ToConstant(this.syncSettings);

            this.Bind<InterviewPreconditionsServiceSettings>().ToConstant(new InterviewPreconditionsServiceSettings(this.interviewLimitCount));

            this.Bind<Questionnaire>().ToSelf();
            this.Bind<IPlainAggregateRootRepository<Questionnaire>>().To<QuestionnaireRepository>();
            this.Bind<IQuestionnaireExportStructureStorage>().To<QuestionnaireExportStructureStorage>().InSingletonScope();
            this.Bind<IQuestionOptionsRepository>().To<QuestionnaireQuestionOptionsRepository>();

            CommandRegistry
                .Setup<Questionnaire>()
                .ResolvesIdFrom<QuestionnaireCommand>(command => command.QuestionnaireId)
                .InitializesWith<ImportFromDesigner>(aggregate => aggregate.ImportFromDesigner, config => config.ValidatedBy<QuestionnaireNameValidator>())
                .InitializesWith<RegisterPlainQuestionnaire>(aggregate => aggregate.RegisterPlainQuestionnaire)
                .InitializesWith<DeleteQuestionnaire>(aggregate => aggregate.DeleteQuestionnaire)
                .InitializesWith<DisableQuestionnaire>(aggregate => aggregate.DisableQuestionnaire)
                .InitializesWith<CloneQuestionnaire>(aggregate => aggregate.CloneQuestionnaire);

            CommandRegistry
                .Setup<StatefulInterview>()
                .InitializesWith<CreateInterviewFromSynchronizationMetadata>(command => command.InterviewId, (command, aggregate) => aggregate.CreateInterviewFromSynchronizationMetadata(command.Id, command.UserId, command.QuestionnaireId, command.QuestionnaireVersion, command.InterviewStatus, command.FeaturedQuestionsMeta, command.Comments, command.RejectedDateTime, command.InterviewerAssignedDateTime, command.Valid, command.CreatedOnClient))
                .InitializesWith<SynchronizeInterviewEventsCommand>(command => command.InterviewId, aggregate => aggregate.SynchronizeInterviewEvents)
                .InitializesWith<CreateInterview>(command => command.InterviewId, (command, aggregate) => aggregate.CreateInterview(command))
                
                .StatelessHandles<HardDeleteInterview>(command => command.InterviewId, (command, aggregate) => aggregate.HardDelete(command.UserId))

                .Handles<AnswerDateTimeQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerDateTimeQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Answer))
                .Handles<AnswerGeoLocationQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerGeoLocationQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Latitude, command.Longitude, command.Accuracy, command.Altitude, command.Timestamp))
                .Handles<AnswerMultipleOptionsLinkedQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerMultipleOptionsLinkedQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.SelectedRosterVectors))
                .Handles<AnswerMultipleOptionsQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerMultipleOptionsQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.SelectedValues))
                .Handles<AnswerYesNoQuestion>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerYesNoQuestion(command))
                .Handles<AnswerNumericIntegerQuestionCommand>(command => command.InterviewId, aggregate => aggregate.AnswerNumericIntegerQuestion)
                .Handles<AnswerNumericRealQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerNumericRealQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Answer))
                .Handles<AnswerPictureQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerPictureQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.PictureFileName))
                .Handles<AnswerAudioQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerAudioQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.FileName, command.Length))
                .Handles<AnswerQRBarcodeQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerQRBarcodeQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Answer))
                .Handles<AnswerSingleOptionLinkedQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerSingleOptionLinkedQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.SelectedRosterVector))
                .Handles<AnswerSingleOptionQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerSingleOptionQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.SelectedValue))
                .Handles<AnswerTextListQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerTextListQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Answers))
                .Handles<AnswerTextQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerTextQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Answer))
                .Handles<RemoveAnswerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.RemoveAnswer(command.QuestionId, command.RosterVector, command.UserId, command.RemoveTime))

                .Handles<ApproveInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Approve(command.UserId, command.Comment, command.ApproveTime))
                .Handles<AssignInterviewerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AssignInterviewer(command.UserId, command.InterviewerId, command.AssignTime))
                .Handles<AssignSupervisorCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AssignSupervisor(command.UserId, command.SupervisorId))
                .Handles<AssignResponsibleCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AssignResponsible(command))
                .Handles<CommentAnswerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.CommentAnswer(command.UserId, command.QuestionId, command.RosterVector, command.CommentTime, command.Comment))
                .Handles<DeleteInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Delete(command.UserId))
                .Handles<HqApproveInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.HqApprove(command.UserId, command.Comment))
                .Handles<HqRejectInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.HqReject(command.UserId, command.Comment))
                .Handles<UnapproveByHeadquartersCommand>(command => command.InterviewId, (command, aggregate) => aggregate.UnapproveByHeadquarters(command.UserId, command.Comment))
                .Handles<MarkInterviewAsReceivedByInterviewer>(command => command.InterviewId, (command, aggregate) => aggregate.MarkInterviewAsReceivedByInterviwer(command.UserId))
                .Handles<ReevaluateSynchronizedInterview>(command => command.InterviewId, (command, aggregate) => aggregate.ReevaluateSynchronizedInterview())
                .Handles<RepeatLastInterviewStatus>(command => command.InterviewId, aggregate => aggregate.RepeatLastInterviewStatus)
                .Handles<RejectInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Reject(command.UserId, command.Comment, command.RejectTime))
                .Handles<RejectInterviewToInterviewerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.RejectToInterviewer(command.UserId, command.InterviewerId, command.Comment, command.RejectTime))
                .Handles<RejectInterviewFromHeadquartersCommand>(command => command.InterviewId, (command, aggregate) => aggregate.RejectInterviewFromHeadquarters(command.UserId, command.SupervisorId, command.InterviewerId, command.InterviewDto, command.SynchronizationTime))
                .Handles<RestartInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Restart(command.UserId, command.Comment, command.RestartTime))
                .Handles<RestoreInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Restore(command.UserId))
                                //.Handles<SynchronizeInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.SynchronizeInterview(command.UserId, command.SynchronizedInterview))
                .Handles<CompleteInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.CompleteWithoutFirePassiveEvents(command.UserId, command.Comment, command.CompleteTime))
                .Handles<SwitchTranslation>(command => command.InterviewId, aggregate => aggregate.SwitchTranslation);

            CommandRegistry.Configure<StatefulInterview, SynchronizeInterviewEventsCommand>(configuration => configuration.ValidatedBy<SurveyManagementInterviewCommandValidator>());
            CommandRegistry.Configure<StatefulInterview, CreateInterview>(configuration => configuration.ValidatedBy<SurveyManagementInterviewCommandValidator>());
            
            this.Bind<IAndroidPackageReader>().To<AndroidPackageReader>();
           
            this.Bind<IPreloadingTemplateService>().To<AssignmentImportTemplateGenerator>().WithConstructorArgument("folderPath", this.currentFolderPath);
            this.Bind<IPreloadedDataRepository>().To<FilebasedPreloadedDataRepository>().InSingletonScope().WithConstructorArgument("folderPath", this.currentFolderPath);
            this.Bind<IPreloadedDataVerifier>().To<ImportDataVerifier>();
            this.Bind<IQuestionDataParser>().To<QuestionDataParser>();
            this.Bind<IPreloadedDataService>().To<ImportDataParsingService>();

            this.Bind<IExportFileNameService>().To<ExportExportFileNameService>();

            //commented because auto registered somewhere 
            //this.Bind<IMetaDescriptionFactory>().To<MetaDescriptionFactory>();
            this.Bind<IRecordsAccessorFactory>().To<CsvRecordsAccessorFactory>();
            this.Bind<IPreloadedDataServiceFactory>().To<PreloadedDataServiceFactory>();
            this.Bind<IBrokenInterviewPackagesViewFactory>().To<BrokenInterviewPackagesViewFactory>();
            this.Bind<ISynchronizationLogViewFactory>().To<SynchronizationLogViewFactory>();
            this.Bind<IInterviewsToDeleteFactory>().To<InterviewsToDeleteFactory>();
            this.Bind<Func<IInterviewsToDeleteFactory>>().ToMethod(context => () => context.Kernel.Get<IInterviewsToDeleteFactory>());
            this.Bind<IInterviewHistoryFactory>().To<InterviewHistoryFactory>();
            this.Bind<IInterviewInformationFactory>().To<InterviewerInterviewsFactory>();
            this.Bind<IDdiMetadataFactory>().To<DdiMetadataFactory>();
            this.Bind<IMetaDescriptionFactory>().To<MetaDescriptionFactory>();
            this.Bind<IDatasetWriterFactory>().To<DatasetWriterFactory>();
            this.Bind<IDataQueryFactory>().To<DataQueryFactory>();
            this.Bind<IQuestionnaireLabelFactory>().To<QuestionnaireLabelFactory>();
            this.Bind<IExportViewFactory>().To<ExportViewFactory>();
            this.Bind<IQuestionnaireVersionProvider>().To<QuestionnaireVersionProvider>();
            this.Bind<ITranslationManagementService>().To<TranslationManagementService>();
            
            this.Bind<IAllInterviewsFactory>().To<AllInterviewsFactory>();
            this.Bind<ITeamInterviewsFactory>().To<TeamInterviewsFactory>();
            this.Bind<IChangeStatusFactory>().To<ChangeStatusFactory>();
            this.Bind<IQuantityReportFactory>().To<QuantityReportFactory>();
            this.Bind<ISpeedReportFactory>().To<SpeedReportFactory>();
            this.Bind<IDeviceInterviewersReport>().To<DeviceInterviewersReport>();
            this.Bind<ISampleUploadViewFactory>().To<SampleUploadViewFactory>();
            this.Bind<IAllUsersAndQuestionnairesFactory>().To<AllUsersAndQuestionnairesFactory>();
            this.Bind<IQuestionnairePreloadingDataViewFactory>().To<QuestionnairePreloadingDataViewFactory>();
            this.Bind<ITeamViewFactory>().To<TeamViewFactory>();
            this.Bind<IUserViewFactory>().ToMethod(context => new UserViewFactory());
            this.Bind<ITeamUsersAndQuestionnairesFactory>().To<TeamUsersAndQuestionnairesFactory>();
            this.Bind<IInterviewDetailsViewFactory>().To<InterviewDetailsViewFactory>();
            this.Bind<IInterviewFactory>().To<InterviewFactory>();
            this.Bind<IInterviewSummaryViewFactory>().To<InterviewSummaryViewFactory>();
            this.Bind<IChartStatisticsViewFactory>().To<ChartStatisticsViewFactory>();
            this.Bind<IQuestionnaireBrowseViewFactory>().To<QuestionnaireBrowseViewFactory>();
            this.Bind<ISampleWebInterviewService>().To<SampleWebInterviewService>();

            this.Bind<IInterviewImportDataParsingService>().To<InterviewImportDataParsingService>();

            this.Bind<IOldschoolChartStatisticsDataProvider>().To<OldschoolChartStatisticsDataProvider>();

            this.Bind<ITeamsAndStatusesReport>().To<TeamsAndStatusesReport>();
            this.Bind<ISurveysAndStatusesReport>().To<SurveysAndStatusesReport>();
            this.Bind<IMapReport>().To<MapReport>();
            this.Bind<IStatusDurationReport>().To<StatusDurationReport>();

            this.Bind<IInterviewUniqueKeyGenerator>().To<InterviewUniqueKeyGenerator>();
            this.Bind<IRandomValuesSource>().To<RandomValuesSource>().InSingletonScope();

            this.Unbind<ISupportedVersionProvider>();
            this.Bind<ISupportedVersionProvider>().To<SupportedVersionProvider>();

            this.Bind<ISyncProtocolVersionProvider>().To<SyncProtocolVersionProvider>().InSingletonScope();

            this.Bind<SyncPackagesProcessorBackgroundJobSetting>().ToConstant(this.syncPackagesProcessorBackgroundJobSetting);
            this.Bind<InterviewDetailsBackgroundSchedulerTask>().ToSelf();

            this.Bind<ITabletInformationService>().To<FileBasedTabletInformationService>().WithConstructorArgument("parentFolder", this.currentFolderPath);

            this.Bind<IPasswordHasher>().To<PasswordHasher>().InSingletonScope(); // external class which cannot be put to self-describing module because ninject is not portable

            this.Bind<IExportFactory>().To<ExportFactory>();

            this.Kernel.RegisterDenormalizer<InterviewSummaryCompositeDenormalizer>();
            this.Kernel.RegisterDenormalizer<InterviewEventHandlerFunctional>();
            this.Kernel.RegisterDenormalizer<InterviewLifecycleEventHandler>();
            this.Kernel.RegisterDenormalizer<InterviewExportedCommentariesDenormalizer>();

            this.Kernel.Load(new QuartzNinjectModule());

            this.Bind<IInterviewPackagesService>().To<InterviewPackagesService>();

            this.Bind<IDeleteQuestionnaireService>().To<DeleteQuestionnaireService>().InSingletonScope();
            this.Bind<IAtomicHealthCheck<EventStoreHealthCheckResult>>().To<EventStoreHealthChecker>();
            this.Bind<IAtomicHealthCheck<FolderPermissionCheckResult>>().To<FolderPermissionChecker>().WithConstructorArgument("folderPath", this.currentFolderPath);
            this.Bind<IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult>>().To<NumberOfUnhandledPackagesChecker>();
            this.Bind<IAtomicHealthCheck<ReadSideHealthCheckResult>>().To<ReadSideHealthChecker>();

            this.Bind<IHealthCheckService>().To<HealthCheckService>();
            this.Bind<ISubstitutionService>().To<SubstitutionService>();
            this.Bind<ISubstitutionTextFactory>().To<SubstitutionTextFactory>();

            this.Bind<ITranslationStorage>().To<TranslationStorage>();
            this.Bind<IQuestionnaireTranslator>().To<QuestionnaireTranslator>();
            this.Bind<IQuestionnaireStorage>().To<QuestionnaireStorage>().InSingletonScope(); // has internal cache, so should be singleton


            this.Bind<IAudioFileStorage>().To<AudioFileStorage>();
            this.Bind<IImageFileStorage>().To<ImageFileStorage>()
                .InSingletonScope().WithConstructorArgument("rootDirectoryPath", this.currentFolderPath);

            this.Bind<IInterviewSynchronizationFileStorage>().To<InterviewSynchronizationFileStorage>()
                .InSingletonScope().WithConstructorArgument("rootDirectoryPath", this.currentFolderPath).WithConstructorArgument("syncDirectoryName", this.syncDirectoryName);

            this.Bind<IQuestionnaireAssemblyAccessor>().To<QuestionnaireAssemblyAccessor>().InSingletonScope();
           
            this.Bind<IInterviewExpressionStatePrototypeProvider>().To<InterviewExpressionStatePrototypeProvider>();
            this.Bind<IVariableToUIStringService>().To<VariableToUIStringService>();
            
            this.Bind<UserPreloadingSettings>().ToConstant(this.userPreloadingSettings);

            this.Bind<IUserBatchCreator>().To<UserBatchCreator>();
            this.Bind<IUserPreloadingVerifier>().To<UserPreloadingVerifier>().InSingletonScope();
            this.Bind<IUserPreloadingCleaner>().To<UserPreloadingCleaner>().InSingletonScope();

            this.Bind<SampleImportSettings>().ToConstant(sampleImportSettings);

            this.Bind<InterviewDataExportSettings>().ToConstant(this.interviewDataExportSettings);
            this.Bind<ExportSettings>().ToConstant(this.exportSettings);
            this.Bind<IFilebasedExportedDataAccessor>().To<FilebasedExportedDataAccessor>();

            this.Bind<IDdiMetadataAccessor>().To<DdiMetadataAccessor>();
            this.Bind<IDataExportFileAccessor>().To<DataExportFileAccessor>();
         
            this.Bind<IDataExportProcessesService>().To<DataExportProcessesService>().InSingletonScope();
            this.Bind<IDataExporter>().To<DataExporter>().InSingletonScope();
            this.Bind<InterviewExportredDataRowReader>().ToSelf();

            this.Bind<ITabularDataToExternalStatPackageExportService>().To<TabularDataToExternalStatPackageExportService>();
            this.Bind<ITabFileReader>().To<TabFileReader>();
          

            this.Bind<IEnvironmentContentService>().To<StataEnvironmentContentService>();
       
            this.Bind<IParaDataAccessor>().To<TabularParaDataAccessor>();

            this.Bind<TabularFormatDataExportHandler>().ToSelf();
            this.Bind<TabularFormatParaDataExportProcessHandler>().ToSelf();
            this.Bind<StataFormatExportHandler>().ToSelf();
            this.Bind<SpssFormatExportHandler>().ToSelf();
            this.Bind<BinaryFormatDataExportHandler>().ToSelf();

            this.Bind<ITabularFormatExportService>().To<ReadSideToTabularFormatExportService>();
            this.Bind<ICsvWriterService>().To<CsvWriterService>();
            this.Bind<ICsvWriter>().To<CsvWriter>();
            this.Bind<IDataExportStatusReader>().To<DataExportStatusReader>();

            this.Bind<IExportQuestionService>().To<ExportQuestionService>();

            this.Bind<IRosterStructureService>().To<RosterStructureService>();
            this.Bind<IQuestionnaireImportService>().To<QuestionnaireImportService>();
            this.Bind<DesignerUserCredentials>().ToSelf();

            this.Bind<IWebInterviewConfigurator>().To<WebInterviewConfigurator>();
            this.Bind<IWebInterviewConfigProvider>().To<WebInterviewConfigProvider>();
            
            this.Bind<IDeviceSyncInfoRepository>().To<DeviceSyncInfoRepository>();
            this.Bind<IAssignmentViewFactory>().To<AssignmentViewFactory>();
            this.Bind<IAssignmentsService>().To<AssignmentsService>();
            this.Bind<IAssignmetnsDeletionService>().To<AssignmetnsDeletionService>();
        }
    }
}
