﻿using Ncqrs.Eventing.Storage;
using System;
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
using WB.Core.Infrastructure.Modularity;

namespace WB.Core.BoundedContexts.Headquarters
{
    public class HeadquartersBoundedContextModule : IModule
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
        private readonly TrackingSettings trackingSettings;

        public HeadquartersBoundedContextModule(string currentFolderPath,
            SyncPackagesProcessorBackgroundJobSetting syncPackagesProcessorBackgroundJobSetting,
            UserPreloadingSettings userPreloadingSettings,
            ExportSettings exportSettings,
            InterviewDataExportSettings interviewDataExportSettings,
            SampleImportSettings sampleImportSettings,
            SyncSettings syncSettings,
            TrackingSettings trackingSettings, 
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
            this.trackingSettings = trackingSettings;
        }

        public void Load(IIocRegistry registry)
        {
            registry.BindToConstant<IEventTypeResolver>(() =>
                new EventTypeResolver(
                    typeof(DataCollectionSharedKernelAssemblyMarker).Assembly,
                    typeof(HeadquartersBoundedContextModule).Assembly));

            registry.BindToConstant<SyncSettings>(() => this.syncSettings);
            registry.BindToConstant<TrackingSettings>(() => this.trackingSettings);

            registry.BindToConstant<InterviewPreconditionsServiceSettings>(() => new InterviewPreconditionsServiceSettings(this.interviewLimitCount));

            registry.Bind<Questionnaire>();
            registry.Bind<IPlainAggregateRootRepository<Questionnaire>, QuestionnaireRepository>();
            registry.BindAsSingleton<IQuestionnaireExportStructureStorage, QuestionnaireExportStructureStorage>();
            registry.Bind<IQuestionOptionsRepository, QuestionnaireQuestionOptionsRepository>();

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
                .Handles<SwitchTranslation>(command => command.InterviewId, aggregate => aggregate.SwitchTranslation)
                .Handles<PauseInterviewCommand>(cmd => cmd.InterviewId, a => a.Pause)
                .Handles<ResumeInterviewCommand>(cmd => cmd.InterviewId, a => a.Resume)
                .Handles<OpenInterviewBySupervisorCommand>(cmd => cmd.InterviewId, a => a.OpenBySupevisor)
                .Handles<CloseInterviewBySupervisorCommand>(cmd => cmd.InterviewId, a => a.CloseBySupevisor);

            CommandRegistry.Configure<StatefulInterview, SynchronizeInterviewEventsCommand>(configuration => configuration.ValidatedBy<SurveyManagementInterviewCommandValidator>());
            CommandRegistry.Configure<StatefulInterview, CreateInterview>(configuration => configuration.ValidatedBy<SurveyManagementInterviewCommandValidator>());
            
            registry.Bind<IAndroidPackageReader, AndroidPackageReader>();
           
            registry.BindWithConstructorArgument<IPreloadingTemplateService, AssignmentImportTemplateGenerator>("folderPath", this.currentFolderPath);
            registry.BindAsSingletonWithConstructorArgument<IPreloadedDataRepository, FilebasedPreloadedDataRepository>("folderPath", this.currentFolderPath);
            registry.Bind<IPreloadedDataVerifier, ImportDataVerifier>();
            registry.Bind<IQuestionDataParser, QuestionDataParser>();
            registry.Bind<IPreloadedDataService, ImportDataParsingService>();

            registry.Bind<IExportFileNameService, ExportExportFileNameService>();

            registry.BindAsSingletonWithConstructorArgument<IMapStorageService, FileSystemMapStorageService>("folderPath", this.currentFolderPath);

            //commented because auto registered somewhere 
            //registry.Bind<IMetaDescriptionFactory>().To<MetaDescriptionFactory>();
            registry.Bind<IRecordsAccessorFactory, CsvRecordsAccessorFactory>();
            registry.Bind<IPreloadedDataServiceFactory, PreloadedDataServiceFactory>();
            registry.Bind<IBrokenInterviewPackagesViewFactory, BrokenInterviewPackagesViewFactory>();
            registry.Bind<ISynchronizationLogViewFactory, SynchronizationLogViewFactory>();
            registry.Bind<IInterviewsToDeleteFactory, InterviewsToDeleteFactory>();
            registry.BindToMethod<Func<IInterviewsToDeleteFactory>>(context => () => context.Get<IInterviewsToDeleteFactory>());
            registry.Bind<IInterviewHistoryFactory, InterviewHistoryFactory>();
            registry.Bind<IInterviewInformationFactory, InterviewerInterviewsFactory>();
            registry.Bind<IDdiMetadataFactory, DdiMetadataFactory>();
            registry.Bind<IMetaDescriptionFactory, MetaDescriptionFactory>();
            registry.Bind<IDatasetWriterFactory, DatasetWriterFactory>();
            registry.Bind<IDataQueryFactory, DataQueryFactory>();
            registry.Bind<IQuestionnaireLabelFactory, QuestionnaireLabelFactory>();
            registry.Bind<IExportViewFactory, ExportViewFactory>();
            registry.Bind<IQuestionnaireVersionProvider, QuestionnaireVersionProvider>();
            registry.Bind<ITranslationManagementService, TranslationManagementService>();
            
            registry.Bind<IAllInterviewsFactory, AllInterviewsFactory>();
            registry.Bind<ITeamInterviewsFactory, TeamInterviewsFactory>();
            registry.Bind<IChangeStatusFactory, ChangeStatusFactory>();
            registry.Bind<IQuantityReportFactory, QuantityReportFactory>();
            registry.Bind<ISpeedReportFactory, SpeedReportFactory>();
            registry.Bind<IDeviceInterviewersReport, DeviceInterviewersReport>();
            registry.Bind<ISampleUploadViewFactory, SampleUploadViewFactory>();
            registry.Bind<IAllUsersAndQuestionnairesFactory, AllUsersAndQuestionnairesFactory>();
            registry.Bind<IQuestionnairePreloadingDataViewFactory, QuestionnairePreloadingDataViewFactory>();
            registry.Bind<ITeamViewFactory, TeamViewFactory>();
            registry.BindToMethod<IUserViewFactory>(context => new UserViewFactory());
            registry.Bind<ITeamUsersAndQuestionnairesFactory, TeamUsersAndQuestionnairesFactory>();
            registry.Bind<IInterviewDetailsViewFactory, InterviewDetailsViewFactory>();
            registry.Bind<IInterviewFactory, InterviewFactory>();
            registry.Bind<IInterviewSummaryViewFactory, InterviewSummaryViewFactory>();
            registry.Bind<IChartStatisticsViewFactory, ChartStatisticsViewFactory>();
            registry.Bind<IQuestionnaireBrowseViewFactory, QuestionnaireBrowseViewFactory>();
            registry.Bind<ISampleWebInterviewService, SampleWebInterviewService>();
            registry.Bind<IMapBrowseViewFactory, MapBrowseViewFactory>();
            

            registry.Bind<IInterviewImportDataParsingService, InterviewImportDataParsingService>();

            registry.Bind<IOldschoolChartStatisticsDataProvider, OldschoolChartStatisticsDataProvider>();

            registry.Bind<ITeamsAndStatusesReport, TeamsAndStatusesReport>();
            registry.Bind<ISurveysAndStatusesReport, SurveysAndStatusesReport>();
            registry.Bind<IMapReport, MapReport>();
            registry.Bind<IStatusDurationReport, StatusDurationReport>();

            registry.Bind<IInterviewUniqueKeyGenerator, InterviewUniqueKeyGenerator>();
            registry.BindAsSingleton<IRandomValuesSource, RandomValuesSource>();

            registry.Unbind<ISupportedVersionProvider>();
            registry.Bind<ISupportedVersionProvider, SupportedVersionProvider>();

            registry.BindAsSingleton<ISyncProtocolVersionProvider, SyncProtocolVersionProvider>();

            registry.BindToConstant<SyncPackagesProcessorBackgroundJobSetting>(() => this.syncPackagesProcessorBackgroundJobSetting);
            registry.Bind<InterviewDetailsBackgroundSchedulerTask>();

            registry.BindWithConstructorArgument<ITabletInformationService, FileBasedTabletInformationService>("parentFolder", this.currentFolderPath);

            registry.BindAsSingleton<IPasswordHasher, PasswordHasher>(); // external class which cannot be put to self-describing module because ninject is not portable

            registry.Bind<IExportFactory, ExportFactory>();

            registry.RegisterDenormalizer<InterviewSummaryCompositeDenormalizer>();
            //registry.Kernel.RegisterDenormalizer<InterviewEventHandlerFunctional>();
            registry.RegisterDenormalizer<InterviewLifecycleEventHandler>();
            registry.RegisterDenormalizer<InterviewExportedCommentariesDenormalizer>();
            registry.RegisterDenormalizer<InterviewDenormalizer>();

            registry.Bind<IInterviewPackagesService, InterviewPackagesService>();

            registry.BindAsSingleton<IDeleteQuestionnaireService, DeleteQuestionnaireService>();
            registry.Bind<IAtomicHealthCheck<EventStoreHealthCheckResult>, EventStoreHealthChecker>();
            registry.BindWithConstructorArgument<IAtomicHealthCheck<FolderPermissionCheckResult>, FolderPermissionChecker>("folderPath", this.currentFolderPath);
            registry.Bind<IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult>, NumberOfUnhandledPackagesChecker>();
            registry.Bind<IAtomicHealthCheck<ReadSideHealthCheckResult>, ReadSideHealthChecker>();

            registry.Bind<IHealthCheckService, HealthCheckService>();
            registry.Bind<ISubstitutionService, SubstitutionService>();
            registry.Bind<ISubstitutionTextFactory, SubstitutionTextFactory>();

            registry.Bind<ITranslationStorage, TranslationStorage>();
            registry.Bind<IQuestionnaireTranslator, QuestionnaireTranslator>();
            registry.BindAsSingleton<IQuestionnaireStorage, QuestionnaireStorage>(); // has internal cache, so should be singleton


            registry.Bind<IAudioFileStorage, AudioFileStorage>();
            registry.BindAsSingletonWithConstructorArgument<IImageFileStorage, ImageFileStorage>("rootDirectoryPath", this.currentFolderPath);

            registry.BindAsSingletonWithConstructorArgument<IInterviewSynchronizationFileStorage, InterviewSynchronizationFileStorage>(
                new[]
                    {
                        new ConstructorArgument("rootDirectoryPath", _ => this.currentFolderPath),
                        new ConstructorArgument("syncDirectoryName", _ => this.syncDirectoryName)
                    });

            registry.BindAsSingleton<IQuestionnaireAssemblyAccessor, QuestionnaireAssemblyAccessor>();
           
            registry.Bind<IInterviewExpressionStatePrototypeProvider, InterviewExpressionStatePrototypeProvider>();
            registry.Bind<IVariableToUIStringService, VariableToUIStringService>();
            
            registry.BindToConstant<UserPreloadingSettings>(() => this.userPreloadingSettings);
            
            registry.Bind<IUserImportVerifier, UserImportVerifier>();

            registry.BindToConstant<SampleImportSettings>(() => sampleImportSettings);

            registry.BindToConstant<InterviewDataExportSettings>(() => this.interviewDataExportSettings);
            registry.BindToConstant<ExportSettings>(() => this.exportSettings);
            registry.Bind<IFilebasedExportedDataAccessor, FilebasedExportedDataAccessor>();

            registry.Bind<IDdiMetadataAccessor, DdiMetadataAccessor>();
            registry.Bind<IDataExportFileAccessor, DataExportFileAccessor>();
         
            registry.BindAsSingleton<IDataExportProcessesService, DataExportProcessesService>();
            registry.Bind<IInterviewErrorsExporter, InterviewErrorsExporter>();

            registry.Bind<ITabularDataToExternalStatPackageExportService, TabularDataToExternalStatPackageExportService>();
            registry.Bind<ITabFileReader, TabFileReader>();
          

            registry.Bind<IEnvironmentContentService, StataEnvironmentContentService>();

            registry.Bind<TabularFormatDataExportHandler>();
            registry.Bind<TabularFormatParaDataExportProcessHandler>();
            registry.Bind<StataFormatExportHandler>();
            registry.Bind<SpssFormatExportHandler>();
            registry.Bind<BinaryFormatDataExportHandler>();

            registry.Bind<ITabularFormatExportService, ReadSideToTabularFormatExportService>();
            registry.Bind<ICsvWriterService, CsvWriterService>();
            registry.Bind<ICsvWriter, CsvWriter>();
            registry.Bind<ICsvReader, CsvReader>();
            registry.Bind<IDataExportStatusReader, DataExportStatusReader>();
            registry.Bind<IInterviewsExporter, InterviewsExporter>();

            registry.Bind<IExportQuestionService, ExportQuestionService>();

            registry.Bind<IRosterStructureService, RosterStructureService>();
            registry.Bind<IQuestionnaireImportService, QuestionnaireImportService>();
            registry.Bind<DesignerUserCredentials>();

            registry.Bind<IWebInterviewConfigurator, WebInterviewConfigurator>();
            registry.Bind<IWebInterviewConfigProvider, WebInterviewConfigProvider>();
            
            registry.Bind<IDeviceSyncInfoRepository, DeviceSyncInfoRepository>();
            registry.Bind<IAssignmentViewFactory, AssignmentViewFactory>();
            registry.Bind<IAssignmentsService, AssignmentsService>();
            registry.Bind<IAssignmetnsDeletionService, AssignmetnsDeletionService>();
            registry.Bind<IAuditLog, AuditLog>();
            registry.Bind<IAuditLogReader, AuditLogReader>();

            registry.BindAsSingleton<IPauseResumeQueue, PauseResumeQueue>();
        }
    }
}
