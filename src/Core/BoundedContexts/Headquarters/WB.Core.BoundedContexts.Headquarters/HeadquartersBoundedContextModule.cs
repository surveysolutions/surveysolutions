using Ncqrs.Eventing.Storage;
using System.Threading.Tasks;
using MediatR;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Templates;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Assignments.Validators;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents.Validators;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.CompletedEmails;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.DynamicReporting;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.TabletInformation;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Interview.Postprocessors;
using WB.Core.BoundedContexts.Headquarters.Interview.Preprocessors;
using WB.Core.BoundedContexts.Headquarters.Interview.Validators;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Jobs;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Services.DynamicReporting;
using WB.Core.BoundedContexts.Headquarters.Services.Internal;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Users.MoveUserToAnotherTeam;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Jobs;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile.InterviewerAuditLog;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Reports;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Data;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.BoundedContexts.Headquarters.WebInterview.Impl;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.BoundedContexts.Headquarters.Workspaces.Impl;
using WB.Core.BoundedContexts.Headquarters.Workspaces.Jobs;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.Infrastructure.HttpServices.Services;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Core.SharedKernels.Questionnaire.ReusableCategories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;
using WB.Core.Synchronization.MetaInfo;
using WB.Enumerator.Native.Questionnaire;
using WB.Enumerator.Native.Questionnaire.Impl;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Infrastructure.Native.Questionnaire;
using WB.Infrastructure.Native.Questionnaire.Impl;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Workspaces;
using CalendarEvent = WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.CalendarEvent;
using WB.UI.Shared.Web.Services;

namespace WB.Core.BoundedContexts.Headquarters
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class HeadquartersBoundedContextModule : IModule, IInitModule
    {
        private readonly ExternalStoragesSettings externalStoragesSettings;
        private readonly FileSystemEmailServiceSettings fileSystemEmailServiceSettings;
        private readonly SyncPackagesProcessorBackgroundJobSetting syncPackagesJobSetting;
        private readonly UserPreloadingSettings userPreloadingSettings;
        private readonly SampleImportSettings sampleImportSettings;
        private readonly SyncSettings syncSettings;
        private readonly TrackingSettings trackingSettings;

        public HeadquartersBoundedContextModule(
            UserPreloadingSettings userPreloadingSettings,
            SampleImportSettings sampleImportSettings,
            SyncSettings syncSettings,
            TrackingSettings trackingSettings,
            ExternalStoragesSettings externalStoragesSettings = null,
            FileSystemEmailServiceSettings fileSystemEmailServiceSettings = null,
            SyncPackagesProcessorBackgroundJobSetting syncPackagesJobSetting = null)
        {
            this.userPreloadingSettings = userPreloadingSettings;
            this.sampleImportSettings = sampleImportSettings;
            this.syncSettings = syncSettings;
            this.externalStoragesSettings = externalStoragesSettings;
            this.fileSystemEmailServiceSettings = fileSystemEmailServiceSettings;
            this.syncPackagesJobSetting = syncPackagesJobSetting;
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

            registry.Bind<Questionnaire>();
            registry.Bind<IPlainAggregateRootRepository<Questionnaire>, QuestionnaireRepository>();
            registry.Bind<IQuestionnaireExportStructureStorage, QuestionnaireExportStructureStorage>();
            registry.Bind<IQuestionOptionsRepository, QuestionnaireQuestionOptionsRepository>();

            registry.Bind<IAndroidPackageReader, AndroidPackageReader>();
           
            registry.Bind<IPreloadingTemplateService, AssignmentImportTemplateGenerator>();
            registry.Bind<IPreloadedDataVerifier, ImportDataVerifier>();

            registry.Bind<IExportFileNameService, ExportExportFileNameService>();
            registry.Bind<IDeleteQuestionnaireService, DeleteQuestionnaireService>();
            registry.Bind<IAssignmentsImportService, AssignmentsImportService>();
            registry.Bind<IAssignmentsImportFileConverter, AssignmentsImportFileConverter>();
            
            registry.BindAsSingleton<IStringCompressor, JsonCompressor>();
            registry.Bind<ISerializer, NewtonJsonSerializer>();
            registry.Bind<IJsonAllTypesSerializer, JsonAllTypesSerializer>();
            registry.Bind<IAttachmentContentService, AttachmentContentService>();
            registry.Bind<IMoveUserToAnotherTeamService, MoveUserToAnotherTeamService>();

            registry.Bind<IMapStorageService, MapFileStorageService>();

            //commented because auto registered somewhere 
            //registry.Bind<IMetaDescriptionFactory>().To<MetaDescriptionFactory>();
            registry.Bind<IBrokenInterviewPackagesViewFactory, BrokenInterviewPackagesViewFactory>();
            registry.Bind<ISynchronizationLogViewFactory, SynchronizationLogViewFactory>();
            registry.Bind<IInterviewsToDeleteFactory, InterviewsToDeleteFactory>();
            //registry.BindToMethod<Func<IInterviewsToDeleteFactory>>(context => () => context.Get<IInterviewsToDeleteFactory>());
            registry.Bind<IInterviewHistoryFactory, InterviewHistoryFactory>();
            registry.Bind<IInterviewStatisticsReportDenormalizer, InterviewStatisticsReportDenormalizer>();
            registry.Bind<InterviewSummaryCompositeDenormalizer>();
            registry.Bind<InterviewSummaryDenormalizer>();
            registry.Bind<StatusChangeHistoryDenormalizerFunctional>();
            registry.Bind<InterviewStatusTimeSpanDenormalizer>();
            registry.Bind<InterviewGeoLocationAnswersDenormalizer>();
            registry.Bind<InterviewExportedCommentariesDenormalizer>();
            registry.Bind<CumulativeChartDenormalizer>();
            registry.Bind<AssignmentDenormalizer>();
            registry.Bind<CompletedEmailDenormalizer>();
            registry.Bind<IInterviewInformationFactory, InterviewerInterviewsFactory>();
            registry.Bind<InterviewDynamicReportAnswersDenormalizer>();

            registry.Bind<CalendarEventDenormalizer>();
          
            registry.Bind<IQuestionnaireVersionProvider, QuestionnaireVersionProvider>();
            registry.Bind<ITranslationManagementService, TranslationManagementService>();
            registry.Bind<ITranslationImporter, TranslationImporter>();
            registry.Bind<ICategoriesImporter, CategoriesImporter>();
            registry.Bind<IAssemblyService, AssemblyService>();
            registry.Bind<IArchiveUtils, IProtectedArchiveUtils, ZipArchiveUtils>();
            registry.Bind<IReusableCategoriesStorage, ReusableCategoriesStorage>();
            registry.Bind<IReusableCategoriesFillerIntoQuestionnaire, ReusableCategoriesFillerIntoQuestionnaire>();

            registry.Bind<IAllInterviewsFactory, AllInterviewsFactory>();
            registry.Bind<IChangeStatusFactory, ChangeStatusFactory>();
            registry.Bind<IQuantityReportFactory, QuantityReportFactory>();
            registry.Bind<ISpeedReportFactory, SpeedReportFactory>();
            registry.Bind<IDeviceInterviewersReport, DeviceInterviewersReport>();
            registry.Bind<ISampleUploadViewFactory, SampleUploadViewFactory>();
            registry.Bind<ISurveyStatisticsReport, SurveyStatisticsReport>();
            registry.Bind<IAllUsersAndQuestionnairesFactory, AllUsersAndQuestionnairesFactory>();
            registry.Bind<ITeamViewFactory, TeamViewFactory>();
            registry.Bind<IUserViewFactory, UserViewFactory>();

            registry.Bind<ITeamUsersAndQuestionnairesFactory, TeamUsersAndQuestionnairesFactory>();
            registry.Bind<IInterviewFactory, InterviewFactory>();
            registry.Bind<IChartStatisticsViewFactory, ChartStatisticsViewFactory>();
            registry.Bind<IQuestionnaireBrowseViewFactory, QuestionnaireBrowseViewFactory>();
            registry.Bind<ISampleWebInterviewService, SampleWebInterviewService>();
            registry.Bind<IMapBrowseViewFactory, MapBrowseViewFactory>();
            registry.Bind<IInterviewDiagnosticsFactory, InterviewDiagnosticsFactory>();
            registry.Bind<IUserArchiveService, UserArchiveService>();
            registry.Bind<IMoveUserToAnotherTeamService, MoveUserToAnotherTeamService>();

            registry.Bind<ITeamsAndStatusesReport, TeamsAndStatusesReport>();
            registry.Bind<ISurveysAndStatusesReport, SurveysAndStatusesReport>();
            registry.Bind<IMapReport, MapReport>();
            registry.Bind<IStatusDurationReport, StatusDurationReport>();
            registry.BindAsSingleton<ICommandsMonitoring, PrometheusCommandsMonitoring>();
            registry.Bind<IInterviewUniqueKeyGenerator, InterviewUniqueKeyGenerator>();
            registry.BindAsSingleton<IRandomValuesSource, RandomValuesSource>();

            registry.Bind<ISupportedVersionProvider, SupportedVersionProvider>();

            registry.BindAsSingleton<IInterviewerSyncProtocolVersionProvider, InterviewerSyncProtocolVersionProvider>();
            registry.BindAsSingleton<ISupervisorSyncProtocolVersionProvider, SupervisorSyncProtocolVersionProvider>();

            registry.Bind<IEnumeratorGroupStateCalculationStrategy, EnumeratorGroupGroupStateCalculationStrategy>();
            registry.Bind<ISupervisorGroupStateCalculationStrategy, SupervisorGroupStateCalculationStrategy>();

            registry.Bind<ITabletInformationService, FileBasedTabletInformationService>();

            registry.BindAsSingleton<IPasswordHasher, PasswordHasher>(); 

            registry.Bind<IExportFactory, ExportFactory>();

            registry.RegisterDenormalizer<AssignmentDenormalizer>();
            registry.RegisterDenormalizer<CalendarEventDenormalizer>();
            registry.RegisterDenormalizer<InterviewSummaryCompositeDenormalizer>();
            registry.RegisterDenormalizer<CumulativeChartDenormalizer>();
            registry.RegisterDenormalizer<CompletedEmailDenormalizer>();
            registry.Bind<InterviewCacheWarmupPreProcessor>();
            registry.Bind<InterviewSummaryErrorsCountPostProcessor>();
            registry.Bind<InterviewReceivedByInterviewerCommandValidator>();
            registry.Bind<QuestionnaireValidator>();
            registry.Bind<ResponsibleAssignmentValidator>();

            registry.Bind<QuestionnaireStateForAssignmentValidator>();
            registry.Bind<QuestionnaireStateForCalendarEventValidator>();
            registry.Bind<QuestionnaireStateForInterviewValidator>();
            registry.Bind<AssignmentLimitInterviewValidator>();

            registry.Bind<IInterviewPackagesService, IInterviewBrokenPackagesService, InterviewPackagesService>();
            registry.Bind<ICalendarEventPackageService, CalendarEventPackageService>();
            
            registry.Bind<IExposedVariablesService, ExposedVariablesService>();
            registry.Bind<ISubstitutionService, SubstitutionService>();
            registry.Bind<ISubstitutionTextFactory, SubstitutionTextFactory>();

            registry.Bind<ITranslationStorage, TranslationStorage>();
            registry.Bind<ITranslationsExportService, TranslationsExportService>();
            registry.Bind<ICategoriesExportService, CategoriesExportService>();
            registry.Bind<IQuestionnaireTranslator, QuestionnaireTranslator>();
            registry.Bind<IQuestionnaireStorage, HqQuestionnaireStorage>(); 
           
            registry.Bind<IInterviewExpressionStorageProvider, InterviewExpressionStorageProvider>();
            registry.Bind<IVariableToUIStringService, VariableToUIStringService>();
            
            registry.BindToConstant<UserPreloadingSettings>(() => this.userPreloadingSettings);
            
            registry.Bind<IUserImportVerifier, UserImportVerifier>();

            registry.BindToConstant<SampleImportSettings>(() => sampleImportSettings);

            registry.Bind<IRosterStructureService, RosterStructureService>();
            registry.Bind<IQuestionnaireImportService, QuestionnaireImportService>();
            registry.BindAsSingleton<IQuestionnaireImportStatuses, QuestionnaireImportStatuses>();

            registry.Bind<IWebInterviewConfigurator, WebInterviewConfigurator>();
            registry.Bind<IWebInterviewConfigProvider, WebInterviewConfigProvider>();
            
            registry.Bind<IDeviceSyncInfoRepository, DeviceSyncInfoRepository>();
            registry.Bind<IAssignmentIdGenerator, AssignmentIdGenerator>();
            registry.Bind<IAssignmentsToDeleteFactory, AssignmentsToDeleteFactory>();
            registry.Bind<IAssignmentViewFactory, AssignmentViewFactory>();
            registry.Bind<IAssignmentsService, AssignmentsService>();
            registry.Bind<ISystemLog, SystemLog>();
            registry.Bind<IUserToDeviceService, UserToDeviceService>();
            registry.Bind<IEncryptionService, RsaEncryptionService>();
            registry.Bind<IMetaInfoBuilder, MetaInfoBuilder>();
            registry.Bind<IAssignmentsImportReader, AssignmentsImportReader>();
            registry.Bind<ICompletedEmailsQueue , CompletedEmailsQueue >();

            registry.Bind<ICalendarEventService, CalendarEventService>();
            
            registry.Bind<IAuditLogFactory, AuditLogFactory>();
            registry.Bind<IAuditLogService, AuditLogService>();
            registry.BindToConstant<IAuditLogTypeResolver>(() => new AuditLogTypeResolver(typeof(IAuditLogEntity).Assembly));

            registry.Bind<IAssignmentsUpgradeService, AssignmentsUpgradeService>();
            registry.Bind<ISingleAssignmentUpgrader, SingleAssignmentUpgrader>();
            registry.Bind<IAssignmentsUpgrader, AssignmentsUpgrader>();
            registry.Bind<IAssignmentFactory, AssignmentFactory>();
            registry.Bind<IAssignmentPasswordGenerator, AssignmentPasswordGenerator>();
            registry.Bind<IInterviewReportDataRepository, InterviewReportDataRepository>();
            registry.Bind<IInterviewTreeBuilder, InterviewTreeBuilder>();
            registry.BindAsSingleton<IInterviewAnswerSerializer, NewtonInterviewAnswerJsonSerializer>();
            registry.BindInPerLifetimeScope<IEventSourcedAggregateRootRepository, EventSourcedAggregateRootRepositoryWithWebCache>();
            
            registry.Bind<ISystemLogViewFactory, SystemLogViewFactory>();
            
            if (fileSystemEmailServiceSettings?.IsEnabled ?? false)
            {
                registry.Bind<IEmailService, FileSystemEmailService>(new ConstructorArgument("settings", _ => fileSystemEmailServiceSettings));
                registry.Bind<IPlainKeyValueStorage<EmailProviderSettings>, FileSystemEmailProviderSettingsStorage>(new ConstructorArgument("settings", _ => fileSystemEmailServiceSettings));
            }
            else
            {
                registry.Bind<IEmailService, EmailService>();
            }

            registry.Bind<IInvitationService, InvitationService>();
            registry.Bind<ITokenGenerator, TokenGenerator>();
            registry.Bind<IInvitationMailingService, InvitationMailingService>();
            registry.Bind<IInvitationsDeletionService, InvitationsDeletionService>();
            registry.Bind<IWebInterviewLinkProvider, WebInterviewLinkProvider>();
            registry.Bind<IUserImportService, UserImportService>();

            registry.BindInPerLifetimeScope<IWorkspaceContextHolder, WorkspaceContextHolder>();
            registry.Bind<IWorkspaceContextAccessor, WorkspaceContextAccessor>();
            registry.Bind<IWorkspaceContextSetter, WorkspaceContextSetter>();
            registry.Bind<IWorkspacesService, WorkspacesService>();
            registry.Bind<IWorkspacesCache, WorkspacesCache>();
            registry.Bind<IWorkspacesUsersCache, WorkspacesUsersCache>();
            registry.BindInPerLifetimeScope<IMemoryCacheSource, WorkspaceAwareMemoryCache>();
            registry.BindToMethod(ctx =>
            {
                var cacheSource = ctx.Resolve<IMemoryCacheSource>();
                var contextAccessor = ctx.Resolve<IWorkspaceContextAccessor>();
                return cacheSource.GetCache(contextAccessor.CurrentWorkspace()?.Name ?? WorkspaceConstants.SchemaName);
            }, externallyOwned: true);
            
            registry.Bind<IInScopeExecutor, UnitOfWorkInScopeExecutor>();
            registry.Bind<IRootScopeExecutor, InRootScopeExecutor>();
            registry.Bind(typeof(IInScopeExecutor<>), typeof(UnitOfWorkInScopeExecutor<>));
            registry.Bind(typeof(IInScopeExecutor<,>), typeof(UnitOfWorkInScopeExecutor<,>));

            registry.BindInPerLifetimeScope<ILiteEventBus, NcqrCompatibleEventDispatcher>();

            registry.BindToConstant<SyncPackagesProcessorBackgroundJobSetting>(() => syncPackagesJobSetting);
            registry.BindToConstant<AssignmentImportOptions>(() => new AssignmentImportOptions(15));
            registry.Bind<AssignmentAggregateRoot>();
            registry.Bind<AssignmentsVerificationTask>();
            registry.Bind<AssignmentsImportTask>();
            registry.Bind<InterviewDetailsBackgroundSchedulerTask>();
            registry.Bind<SendInvitationsTask>();
            registry.Bind<SendRemindersTask>();
            registry.Bind<SyncPackagesReprocessorBackgroundJob>();
            registry.Bind<AssignmentsImportJob>();
            registry.Bind<SendInvitationsJob>();
            registry.Bind<AssignmentsVerificationJob>();
            registry.Bind<SendRemindersJob>();
                
            registry.Bind<SendInterviewCompletedJob>();
            registry.Bind<SendInterviewCompletedTask>();
            registry.Bind<DeleteWorkspaceSchemaJob>();
            
            registry.BindScheduledJob<DeleteWorkspaceSchemaJob, DeleteWorkspaceJobData>();
            registry.BindScheduledJob<DeleteQuestionnaireJob, DeleteQuestionnaireRequest>();
            registry.BindScheduledJob<UpgradeAssignmentJob, AssignmentsUpgradeProcess>();
            registry.BindScheduledJob<UsersImportJob, Unit>();
            registry.BindScheduledJob<UpdateDynamicReportJob, UpdateDynamicReportRequest>();

            registry.Bind<CalendarEvent>();

            registry.Bind<IVirtualPathService, VirtualPathService>();
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            var registry = serviceLocator.GetInstance<IDenormalizerRegistry>();
            registry.RegisterFunctional<InterviewSummaryCompositeDenormalizer>();
            registry.RegisterFunctional<CumulativeChartDenormalizer>();
            registry.RegisterFunctional<AssignmentDenormalizer>();
            registry.Register<CompletedEmailDenormalizer>();
            registry.RegisterFunctional<CalendarEventDenormalizer>();
            
            CommandRegistry
                .Setup<Questionnaire>()
                .ResolvesIdFrom<QuestionnaireCommand>(command => command.QuestionnaireId)
                .InitializesWith<ImportFromDesigner>(aggregate => aggregate.ImportFromDesigner, 
                    config => config.ValidatedBy<QuestionnaireValidator>())
                .InitializesWith<RegisterPlainQuestionnaire>(aggregate => aggregate.RegisterPlainQuestionnaire)
                .InitializesWith<DeleteQuestionnaire>(aggregate => aggregate.DeleteQuestionnaire)
                .InitializesWith<DisableQuestionnaire>(aggregate => aggregate.DisableQuestionnaire)
                .InitializesWith<CloneQuestionnaire>(aggregate => aggregate.CloneQuestionnaire, 
                    config => config.ValidatedBy<QuestionnaireValidator>());

            CommandRegistry
                .Setup<AssignmentAggregateRoot>()
                .ResolvesIdFrom<AssignmentCommand>(command => command.PublicKey)
                .InitializesWith<CreateAssignment>(aggregate => aggregate.CreateAssignment, cfg =>cfg.ValidatedBy<ResponsibleAssignmentValidator>())
                .StatelessHandles<DeleteAssignment>(aggregate => aggregate.DeleteAssignment)

                .Handles<ReassignAssignment>(aggregate => aggregate.Reassign)
                .Handles<ArchiveAssignment>(aggregate => aggregate.Archive)
                .Handles<UnarchiveAssignment>(aggregate => aggregate.Unarchive)
                .Handles<MarkAssignmentAsReceivedByTablet>(aggregate => aggregate.MarkAssignmentAsReceivedByTablet)
                .Handles<UpdateAssignmentAudioRecording>(aggregate => aggregate.UpdateAssignmentAudioRecording)
                .Handles<UpdateAssignmentQuantity>(aggregate => aggregate.UpdateAssignmentQuantity)
                .Handles<UpgradeAssignmentCommand>(aggregate => aggregate.UpgradeAssignment)
                .Handles<UpdateAssignmentWebMode>(aggregate => aggregate.UpdateAssignmentWebMode);

            CommandRegistry.Configure<AssignmentAggregateRoot, AssignmentCommand>(configuration => configuration
                .ValidatedBy<QuestionnaireStateForAssignmentValidator>()
                    .SkipValidationFor<DeleteAssignment>());

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
                .Handles<ResolveCommentAnswerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.ResolveComment(command))
                .Handles<CommentAnswerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.CommentAnswer(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Comment))
                .Handles<DeleteInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Delete(command.UserId, command.OriginDate))
                .Handles<HqApproveInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.HqApprove(command.UserId, command.Comment, command.OriginDate))
                .Handles<ChangeInterviewModeCommand>(command => command.InterviewId, (command, aggregate) => aggregate.ChangeInterviewMode(command.UserId, command.OriginDate, command.Mode))
                .Handles<HqRejectInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.HqReject(command.UserId, command.Comment, command.OriginDate))
                .Handles<HqRejectInterviewToSupervisorCommand>(command => command.InterviewId, (command, aggregate) => aggregate.HqRejectInterviewToSupervisor(command.UserId, command.SupervisorId, command.Comment, command.OriginDate))
                .Handles<HqRejectInterviewToInterviewerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.HqRejectInterviewToInterviewer(command.UserId, command.InterviewerId, command.SupervisorId, command.Comment, command.OriginDate))
                .Handles<UnapproveByHeadquartersCommand>(command => command.InterviewId, (command, aggregate) => aggregate.UnapproveByHeadquarters(command.UserId, command.Comment, command.OriginDate))
                .Handles<MarkInterviewAsReceivedByInterviewer>(command => command.InterviewId, (command, aggregate) => aggregate.MarkInterviewAsReceivedByInterviwer(command.UserId, command.OriginDate))
                .Handles<ReevaluateInterview>(command => command.InterviewId, (command, aggregate) => aggregate.ReevaluateInterview(command.UserId, command.OriginDate))
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
                .Handles<OpenInterviewBySupervisorCommand>(cmd => cmd.InterviewId, a => a.OpenBySupervisor)
                .Handles<CloseInterviewBySupervisorCommand>(cmd => cmd.InterviewId, a => a.CloseBySupervisor);
            
            CommandRegistry.Configure<StatefulInterview, InterviewCommand>(configuration => configuration
                .PreProcessBy<InterviewCacheWarmupPreProcessor>()
                    .SkipPreProcessFor<HardDeleteInterview>()
                    .SkipPreProcessFor<DeleteInterviewCommand>()
                    .SkipPreProcessFor<MarkInterviewAsReceivedByInterviewer>()
                    .SkipPreProcessFor<AssignInterviewerCommand>()
                    .SkipPreProcessFor<AssignSupervisorCommand>()
                .PreProcessBy<IWebInterviewTimezoneSetter>()
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
                    .SkipValidationFor<RejectInterviewCommand>()
                    .SkipValidationFor<HqRejectInterviewCommand>()
                    .SkipValidationFor<UnapproveByHeadquartersCommand>()
                    .SkipValidationFor<ApproveInterviewCommand>()
                    .SkipValidationFor<HqApproveInterviewCommand>()
                    .SkipValidationFor<ChangeInterviewModeCommand>()
                .ValidatedBy<AssignmentLimitInterviewValidator>()
                .ValidatedBy<QuestionnaireStateForInterviewValidator>()
                    .SkipValidationFor<HardDeleteInterview>()
            
            );
            
            CommandRegistry
                .Setup<CalendarEvent>()
                .ResolvesIdFrom<CalendarEventCommand>(command => command.PublicKey)
                .InitializesWith<CreateCalendarEventCommand>( aggregate => aggregate.CreateCalendarEvent)
                .InitializesWith<SyncCalendarEventEventsCommand>( aggregate => aggregate.SyncCalendarEventEvents)
                .Handles<DeleteCalendarEventCommand>( aggregate => aggregate.DeleteCalendarEvent)
                .Handles<UpdateCalendarEventCommand>(aggregate => aggregate.UpdateCalendarEvent)
                .Handles<CompleteCalendarEventCommand>(aggregate => aggregate.CompleteCalendarEvent)
                .Handles<RestoreCalendarEventCommand>( aggregate => aggregate.RestoreCalendarEvent);
            
            CommandRegistry.Configure<CalendarEvent, CalendarEventCommand>(configuration => configuration
                .ValidatedBy<QuestionnaireStateForCalendarEventValidator>());

            return Task.CompletedTask;
        }
    }
}
