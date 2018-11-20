using Ncqrs.Eventing.Storage;
using System;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
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
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Services.Export;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Templates;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.InterviewerAuditLog;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services.Internal;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.BoundedContexts.Headquarters.Views.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.Reports;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Data;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.BoundedContexts.Headquarters.WebInterview.Impl;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Modularity;
using WB.Enumerator.Native.Questionnaire;
using WB.Enumerator.Native.Questionnaire.Impl;
using WB.Enumerator.Native.WebInterview;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Infrastructure.Native.Storage;

namespace WB.Core.BoundedContexts.Headquarters
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class HeadquartersBoundedContextModule : IModule
    {
        private readonly string currentFolderPath;
        private readonly SyncPackagesProcessorBackgroundJobSetting syncPackagesProcessorBackgroundJobSetting;
        private readonly int? interviewLimitCount;
        private readonly string syncDirectoryName;
        private readonly ExternalStoragesSettings externalStoragesSettings;
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
            string syncDirectoryName = "SYNC",
            ExternalStoragesSettings externalStoragesSettings = null)
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
            this.externalStoragesSettings = externalStoragesSettings;
            this.trackingSettings = trackingSettings;
        }

        public void Load(IIocRegistry registry)
        {
            registry.BindToConstant<IEventTypeResolver>(() =>
                new EventTypeResolver(
                    typeof(DataCollectionSharedKernelAssemblyMarker).Assembly,
                    typeof(HeadquartersBoundedContextModule).Assembly));

            registry.BindAsSingleton<IInMemoryEventStore, InMemoryEventStore>();

            registry.BindToConstant(() => this.externalStoragesSettings);

            registry.BindToConstant<SyncSettings>(() => this.syncSettings);
            registry.BindToConstant<TrackingSettings>(() => this.trackingSettings);

            registry.BindToConstant<InterviewPreconditionsServiceSettings>(() => new InterviewPreconditionsServiceSettings(this.interviewLimitCount));

            registry.Bind<Questionnaire>();
            registry.Bind<IPlainAggregateRootRepository<Questionnaire>, QuestionnaireRepository>();
            registry.Bind<IQuestionnaireExportStructureStorage, QuestionnaireExportStructureStorage>();
            registry.Bind<IQuestionOptionsRepository, QuestionnaireQuestionOptionsRepository>();

            registry.Bind<IAndroidPackageReader, AndroidPackageReader>();
           
            registry.BindWithConstructorArgument<IPreloadingTemplateService, AssignmentImportTemplateGenerator>("folderPath", this.currentFolderPath);
            registry.Bind<IPreloadedDataVerifier, ImportDataVerifier>();

            registry.Bind<IExportFileNameService, ExportExportFileNameService>();

            registry.BindAsSingleton<IStringCompressor, JsonCompressor>();
            registry.Bind<ISerializer, NewtonJsonSerializer>();
            registry.Bind<IJsonAllTypesSerializer, JsonAllTypesSerializer>();
            registry.Bind<IAttachmentContentService, AttachmentContentService>();
            registry.Bind<IInterviewAnswerSerializer, NewtonInterviewAnswerJsonSerializer>();

            registry.BindWithConstructorArgument<IMapStorageService, FileSystemMapStorageService>("folderPath", this.currentFolderPath);

            //commented because auto registered somewhere 
            //registry.Bind<IMetaDescriptionFactory>().To<MetaDescriptionFactory>();
            registry.Bind<IBrokenInterviewPackagesViewFactory, BrokenInterviewPackagesViewFactory>();
            registry.Bind<ISynchronizationLogViewFactory, SynchronizationLogViewFactory>();
            registry.Bind<IInterviewsToDeleteFactory, InterviewsToDeleteFactory>();
            //registry.BindToMethod<Func<IInterviewsToDeleteFactory>>(context => () => context.Get<IInterviewsToDeleteFactory>());
            registry.Bind<IInterviewHistoryFactory, InterviewHistoryFactory>();
            registry.Bind<IInterviewInformationFactory, InterviewerInterviewsFactory>();
            registry.Bind<IDatasetWriterFactory, DatasetWriterFactory>();
            registry.Bind<IQuestionnaireLabelFactory, QuestionnaireLabelFactory>();
            registry.Bind<IExportViewFactory, ExportViewFactory>();
            registry.Bind<IQuestionnaireVersionProvider, QuestionnaireVersionProvider>();
            registry.Bind<ITranslationManagementService, TranslationManagementService>();
            registry.Bind<IAssemblyService, AssemblyService>();
            registry.Bind<IExportSettings, Implementation.ExportSettings>();
            registry.Bind<IArchiveUtils, IProtectedArchiveUtils, ZipArchiveUtils>();
            
            registry.Bind<IAllInterviewsFactory, AllInterviewsFactory>();
            registry.Bind<ITeamInterviewsFactory, TeamInterviewsFactory>();
            registry.Bind<IChangeStatusFactory, ChangeStatusFactory>();
            registry.Bind<IQuantityReportFactory, QuantityReportFactory>();
            registry.Bind<ISpeedReportFactory, SpeedReportFactory>();
            registry.Bind<IDeviceInterviewersReport, DeviceInterviewersReport>();
            registry.Bind<ISampleUploadViewFactory, SampleUploadViewFactory>();
            registry.Bind<ISurveyStatisticsReport, SurveyStatisticsReport>();
            registry.Bind<IAllUsersAndQuestionnairesFactory, AllUsersAndQuestionnairesFactory>();
            registry.Bind<IQuestionnairePreloadingDataViewFactory, QuestionnairePreloadingDataViewFactory>();
            registry.Bind<ITeamViewFactory, TeamViewFactory>();
            registry.BindToMethod<IUserViewFactory>(context => new UserViewFactory(context.Resolve<IUserRepository>()));
            registry.Bind<ITeamUsersAndQuestionnairesFactory, TeamUsersAndQuestionnairesFactory>();
            registry.Bind<IInterviewFactory, InterviewFactory>();
            registry.Bind<IInterviewSummaryViewFactory, InterviewSummaryViewFactory>();
            registry.Bind<IChartStatisticsViewFactory, ChartStatisticsViewFactory>();
            registry.Bind<IQuestionnaireBrowseViewFactory, QuestionnaireBrowseViewFactory>();
            registry.Bind<ISampleWebInterviewService, SampleWebInterviewService>();
            registry.Bind<IMapBrowseViewFactory, MapBrowseViewFactory>();
            registry.Bind<IOldschoolChartStatisticsDataProvider, OldschoolChartStatisticsDataProvider>();
            registry.Bind<IInterviewDiagnosticsFactory, InterviewDiagnosticsFactory>();
            registry.Bind<IInterviewsToExportViewFactory, InterviewsToExportViewFactory>();

            registry.Bind<ITeamsAndStatusesReport, TeamsAndStatusesReport>();
            registry.Bind<ISurveysAndStatusesReport, SurveysAndStatusesReport>();
            registry.Bind<IMapReport, MapReport>();
            registry.Bind<IStatusDurationReport, StatusDurationReport>();

            registry.Bind<IInterviewUniqueKeyGenerator, InterviewUniqueKeyGenerator>();
            registry.BindAsSingleton<IRandomValuesSource, RandomValuesSource>();

            registry.Bind<ISupportedVersionProvider, SupportedVersionProvider>();

            registry.BindAsSingleton<IInterviewerSyncProtocolVersionProvider, InterviewerSyncProtocolVersionProvider>();
            registry.BindAsSingleton<ISupervisorSyncProtocolVersionProvider, SupervisorSyncProtocolVersionProvider>();

            registry.BindToConstant<SyncPackagesProcessorBackgroundJobSetting>(() => this.syncPackagesProcessorBackgroundJobSetting);
            registry.Bind<InterviewDetailsBackgroundSchedulerTask>();

            registry.Bind<IEnumeratorGroupStateCalculationStrategy, EnumeratorGroupGroupStateCalculationStrategy>();
            registry.Bind<ISupervisorGroupStateCalculationStrategy, SupervisorGroupStateCalculationStrategy>();

            registry.BindWithConstructorArgument<ITabletInformationService, FileBasedTabletInformationService>("parentFolder", this.currentFolderPath);

            registry.BindAsSingleton<IPasswordHasher, PasswordHasher>(); 

            registry.Bind<IExportFactory, ExportFactory>();

            registry.RegisterDenormalizer<InterviewSummaryCompositeDenormalizer>();
            registry.RegisterDenormalizer<InterviewLifecycleEventHandler>();
            registry.RegisterDenormalizer<InterviewExportedCommentariesDenormalizer>();
            registry.RegisterDenormalizer<InterviewDenormalizer>();
            registry.RegisterDenormalizer<CumulativeChartDenormalizer>();

            registry.Bind<IInterviewPackagesService, IInterviewBrokenPackagesService, InterviewPackagesService>();

            registry.Bind<IDeleteQuestionnaireService, DeleteQuestionnaireService>();
            registry.Bind<IAtomicHealthCheck<EventStoreHealthCheckResult>, EventStoreHealthChecker>();
            registry.BindWithConstructorArgument<IAtomicHealthCheck<FolderPermissionCheckResult>, FolderPermissionChecker>("folderPath", this.currentFolderPath);
            registry.Bind<IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult>, NumberOfUnhandledPackagesChecker>();
            registry.Bind<IAtomicHealthCheck<ReadSideHealthCheckResult>, ReadSideHealthChecker>();

            registry.Bind<IHealthCheckService, HealthCheckService>();
            registry.Bind<ISubstitutionService, SubstitutionService>();
            registry.Bind<ISubstitutionTextFactory, SubstitutionTextFactory>();

            registry.Bind<ITranslationStorage, TranslationStorage>();
            registry.Bind<IQuestionnaireTranslator, QuestionnaireTranslator>();
            registry.Bind<IQuestionnaireStorage, HqQuestionnaireStorage>(); 

            registry.Bind<IQuestionnaireAssemblyAccessor, QuestionnaireAssemblyAccessor>();
           
            registry.Bind<IInterviewExpressionStatePrototypeProvider, InterviewExpressionStatePrototypeProvider>();
            registry.Bind<IVariableToUIStringService, VariableToUIStringService>();
            
            registry.BindToConstant<UserPreloadingSettings>(() => this.userPreloadingSettings);
            
            registry.Bind<IUserImportVerifier, UserImportVerifier>();

            registry.BindToConstant<SampleImportSettings>(() => sampleImportSettings);

            registry.BindToConstant<InterviewDataExportSettings>(() => this.interviewDataExportSettings);
            registry.BindToConstant<ExportSettings>(() => this.exportSettings);

            registry.Bind<IDataExportFileAccessor, DataExportFileAccessor>();
         
            //registry.Bind<IDataExportProcessesService, DataExportProcessesService>();

            registry.Bind<ITabularFormatExportService, ReadSideToTabularFormatExportService>();
            registry.Bind<ICsvWriterService, CsvWriterService>();
            registry.Bind<ICsvWriter, CsvWriter>();
            registry.Bind<ICsvReader, CsvReader>();
            registry.Bind<IDataExportStatusReader, DataExportStatusReader>();

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
            registry.Bind<IAuditLog, Services.Internal.AuditLog>();
            registry.Bind<IAuditLogReader, AuditLogReader>();

            registry.BindAsSingleton<IPauseResumeQueue, PauseResumeQueue>();
            registry.Bind<IAuditLogFactory, AuditLogFactory>();
            registry.BindToConstant<IAuditLogTypeResolver>(() => new AuditLogTypeResolver(typeof(IAuditLogEntity).Assembly));

            registry.Bind<IAssignmentsUpgradeService, AssignmentsUpgradeService>();
            registry.Bind<IAssignmentsUpgrader, AssignmentsUpgrader>();
            registry.Bind<IInterviewReportDataRepository, InterviewReportDataRepository>();
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            CommandRegistry
                .Setup<Questionnaire>()
                .ResolvesIdFrom<QuestionnaireCommand>(command => command.QuestionnaireId)
                .InitializesWith<ImportFromDesigner>(aggregate => aggregate.ImportFromDesigner, config => config.ValidatedBy<QuestionnaireImportValidator>())
                .InitializesWith<RegisterPlainQuestionnaire>(aggregate => aggregate.RegisterPlainQuestionnaire)
                .InitializesWith<DeleteQuestionnaire>(aggregate => aggregate.DeleteQuestionnaire)
                .InitializesWith<DisableQuestionnaire>(aggregate => aggregate.DisableQuestionnaire)
                .InitializesWith<CloneQuestionnaire>(aggregate => aggregate.CloneQuestionnaire);

            CommandRegistry
                .Setup<StatefulInterview>()
                .InitializesWith<CreateInterviewFromSynchronizationMetadata>(command => command.InterviewId, (command, aggregate) => aggregate.CreateInterviewFromSynchronizationMetadata(command.Id, command.UserId, command.QuestionnaireId, command.QuestionnaireVersion, command.InterviewStatus, command.FeaturedQuestionsMeta, command.Comments, command.RejectedDateTime, command.InterviewerAssignedDateTime, command.Valid, command.CreatedOnClient, command.OriginDate))
                .InitializesWith<SynchronizeInterviewEventsCommand>(command => command.InterviewId, aggregate => aggregate.SynchronizeInterviewEvents)
                .InitializesWith<CreateInterview>(command => command.InterviewId, (command, aggregate) => aggregate.CreateInterview(command))
                .InitializesWith<CreateTemporaryInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.CreateTemporaryInterview(command))
                .StatelessHandles<HardDeleteInterview>(command => command.InterviewId, (command, aggregate) => aggregate.HardDelete(command.UserId, command.OriginDate))

                .Handles<AnswerDateTimeQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerDateTimeQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Answer))
                .Handles<AnswerGeoLocationQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerGeoLocationQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Latitude, command.Longitude, command.Accuracy, command.Altitude, command.Timestamp))
                .Handles<AnswerMultipleOptionsLinkedQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerMultipleOptionsLinkedQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.SelectedRosterVectors))
                .Handles<AnswerMultipleOptionsQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerMultipleOptionsQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.SelectedValues))
                .Handles<AnswerYesNoQuestion>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerYesNoQuestion(command))
                .Handles<AnswerNumericIntegerQuestionCommand>(command => command.InterviewId, aggregate => aggregate.AnswerNumericIntegerQuestion)
                .Handles<AnswerNumericRealQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerNumericRealQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Answer))
                .Handles<AnswerPictureQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerPictureQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.PictureFileName))
                .Handles<AnswerAudioQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerAudioQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.FileName, command.Length))
                .Handles<AnswerQRBarcodeQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerQRBarcodeQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Answer))
                .Handles<AnswerSingleOptionLinkedQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerSingleOptionLinkedQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.SelectedRosterVector))
                .Handles<AnswerSingleOptionQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerSingleOptionQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.SelectedValue))
                .Handles<AnswerTextListQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerTextListQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Answers))
                .Handles<AnswerTextQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerTextQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Answer))
                .Handles<RemoveAnswerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.RemoveAnswer(command.QuestionId, command.RosterVector, command.UserId, command.OriginDate))

                .Handles<ApproveInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Approve(command.UserId, command.Comment, command.OriginDate))
                .Handles<AssignInterviewerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AssignInterviewer(command.UserId, command.InterviewerId, command.OriginDate))
                .Handles<AssignSupervisorCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AssignSupervisor(command.UserId,command.SupervisorId,command.OriginDate))
                .Handles<MoveInterviewToTeam>(command => command.InterviewId, (command, aggregate) => aggregate.MoveInterviewToTeam(command))
                .Handles<AssignResponsibleCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AssignResponsible(command))
                .Handles<CommentAnswerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.CommentAnswer(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Comment))
                .Handles<DeleteInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Delete(command.UserId, command.OriginDate))
                .Handles<HqApproveInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.HqApprove(command.UserId, command.Comment, command.OriginDate))
                .Handles<HqRejectInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.HqReject(command.UserId, command.Comment, command.OriginDate))
                .Handles<UnapproveByHeadquartersCommand>(command => command.InterviewId, (command, aggregate) => aggregate.UnapproveByHeadquarters(command.UserId, command.Comment, command.OriginDate))
                .Handles<MarkInterviewAsReceivedByInterviewer>(command => command.InterviewId, (command, aggregate) => aggregate.MarkInterviewAsReceivedByInterviwer(command.UserId, command.OriginDate))
                .Handles<ReevaluateSynchronizedInterview>(command => command.InterviewId, (command, aggregate) => aggregate.ReevaluateSynchronizedInterview(command.UserId))
                .Handles<RepeatLastInterviewStatus>(command => command.InterviewId, aggregate => aggregate.RepeatLastInterviewStatus)
                .Handles<RejectInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Reject(command.UserId, command.Comment, command.OriginDate))
                .Handles<RejectInterviewToInterviewerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.RejectToInterviewer(command.UserId, command.InterviewerId, command.Comment, command.OriginDate))
                .Handles<RestartInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Restart(command.UserId, command.Comment, command.RestartTime))
                .Handles<RestoreInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Restore(command.UserId, command.OriginDate))
                //.Handles<SynchronizeInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.SynchronizeInterview(command.UserId, command.SynchronizedInterview))
                .Handles<CompleteInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.CompleteWithoutFirePassiveEvents(command.UserId, command.Comment, command.OriginDate))
                .Handles<SwitchTranslation>(command => command.InterviewId, aggregate => aggregate.SwitchTranslation)
                .Handles<PauseInterviewCommand>(cmd => cmd.InterviewId, a => a.Pause)
                .Handles<ResumeInterviewCommand>(cmd => cmd.InterviewId, a => a.Resume)
                .Handles<OpenInterviewBySupervisorCommand>(cmd => cmd.InterviewId, a => a.OpenBySupevisor)
                .Handles<CloseInterviewBySupervisorCommand>(cmd => cmd.InterviewId, a => a.CloseBySupevisor);
            
            CommandRegistry.Configure<StatefulInterview, InterviewCommand>(configuration => 
                configuration
                .PostProcessBy<InterviewSummaryErrorsCountPostProcessor>()
                    .SkipPostProcessFor<HardDeleteInterview>()
                    .SkipPostProcessFor<DeleteInterviewCommand>()
                    .SkipPostProcessFor<MarkInterviewAsReceivedByInterviewer>()
                    .SkipPostProcessFor<AssignInterviewerCommand>()
                    .SkipPostProcessFor<AssignSupervisorCommand>()
                .ValidatedBy<InterviewReceivedByInterviewerCommandValidator>()
                    .SkipValidationFor<SynchronizeInterviewEventsCommand>()
                    .SkipValidationFor<MarkInterviewAsReceivedByInterviewer>()
                    .SkipValidationFor<AssignInterviewerCommand>()
                    .SkipValidationFor<AssignResponsibleCommand>()
                    .SkipValidationFor<AssignSupervisorCommand>()
                    .SkipValidationFor<HardDeleteInterview>()
                    .SkipValidationFor<DeleteInterviewCommand>()
                    .SkipValidationFor<MoveInterviewToTeam>()
            );

            CommandRegistry.Configure<StatefulInterview, SynchronizeInterviewEventsCommand>(configuration => configuration.ValidatedBy<SurveyManagementInterviewCommandValidator>());
            CommandRegistry.Configure<StatefulInterview, CreateInterview>(configuration => configuration.ValidatedBy<SurveyManagementInterviewCommandValidator>());

            return Task.CompletedTask;
        }
    }
}
