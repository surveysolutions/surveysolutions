using Ncqrs.Eventing.Storage;
using System;
using Ninject;
using Ninject.Modules;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.DeleteSupervisor;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.HealthCheck;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.HealthCheck.Checks;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.TabletInformation;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Implementation.TemporaryDataStorage;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteSupervisor;
using WB.Core.BoundedContexts.Headquarters.Services.HealthCheck;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.BoundedContexts.Headquarters.UserPreloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.HealthCheck;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Supervisor;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
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
using WB.Core.BoundedContexts.Headquarters.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Translations;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Translations.Impl;
using WB.Core.Synchronization.Implementation.ImportManager;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.TakeNew;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.BoundedContexts.Headquarters.Views.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.Revalidate;
using WB.Core.BoundedContexts.Headquarters.Views.Interviewer;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.Factories;

namespace WB.Core.BoundedContexts.Headquarters
{
    public class HeadquartersBoundedContextModule : NinjectModule
    {
        private readonly string currentFolderPath;
        private readonly InterviewDetailsDataLoaderSettings interviewDetailsDataLoaderSettings;
        private readonly int? interviewLimitCount;
        private readonly ReadSideSettings readSideSettings;
        private readonly string syncDirectoryName;
        private readonly string questionnaireAssembliesDirectoryName;
        private readonly UserPreloadingSettings userPreloadingSettings;
        private readonly ExportSettings exportSettings;
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly SampleImportSettings sampleImportSettings;
        private readonly SyncSettings syncSettings;

        public HeadquartersBoundedContextModule(string currentFolderPath,
            InterviewDetailsDataLoaderSettings interviewDetailsDataLoaderSettings,
            ReadSideSettings readSideSettings,
            UserPreloadingSettings userPreloadingSettings,
            ExportSettings exportSettings,
            InterviewDataExportSettings interviewDataExportSettings,
            SampleImportSettings sampleImportSettings,
            SyncSettings syncSettings,
            int? interviewLimitCount = null,
            string syncDirectoryName = "SYNC",
            string questionnaireAssembliesFolder = "QuestionnaireAssemblies")
        {
            this.userPreloadingSettings = userPreloadingSettings;
            this.exportSettings = exportSettings;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.sampleImportSettings = sampleImportSettings;
            this.currentFolderPath = currentFolderPath;
            this.interviewDetailsDataLoaderSettings = interviewDetailsDataLoaderSettings;
            this.readSideSettings = readSideSettings;
            this.interviewLimitCount = interviewLimitCount;
            this.syncSettings = syncSettings;
            this.syncDirectoryName = syncDirectoryName;
            this.questionnaireAssembliesDirectoryName = questionnaireAssembliesFolder;
        }

        public override void Load()
        {
            this.Bind<IEventTypeResolver>().ToConstant(
                new EventTypeResolver(
                    typeof(DataCollectionSharedKernelAssemblyMarker).Assembly,
                    typeof(HeadquartersBoundedContextModule).Assembly));

            this.Bind<IBackupManager>().To<DefaultBackupManager>();
            this.Bind<SyncSettings>().ToConstant(this.syncSettings);
            //this.Bind<IMetaInfoBuilder>().To<MetaInfoBuilder>();

            CommandRegistry.Setup<Tablet>()
                .InitializesWith<RegisterTabletCommand>(command => command.DeviceId, (command, aggregate) => aggregate.CreateClientDevice(command));

            this.Bind<InterviewPreconditionsServiceSettings>().ToConstant(new InterviewPreconditionsServiceSettings(this.interviewLimitCount));

            this.Bind<Questionnaire>().ToSelf();
            this.Bind<IPlainAggregateRootRepository<Questionnaire>>().To<QuestionnaireRepository>();
            this.Bind<IQuestionnaireExportStructureStorage>().To<QuestionnaireExportStructureStorage>().InSingletonScope();
            this.Bind<IQuestionnaireRosterStructureStorage>().To<QuestionnaireRosterStructureStorage>().InSingletonScope();
            this.Bind<IQuestionOptionsRepository>().To<QuestionnaireQuestionOptionsRepository>();

            CommandRegistry
                .Setup<Questionnaire>()
                .ResolvesIdFrom<QuestionnaireCommand>(command => command.QuestionnaireId)
                .InitializesWith<ImportFromDesigner>(aggregate => aggregate.ImportFromDesigner, config => config.ValidatedBy<QuestionnaireNameValidator>())
                .InitializesWith<RegisterPlainQuestionnaire>(aggregate => aggregate.RegisterPlainQuestionnaire)
                .InitializesWith<DeleteQuestionnaire>(aggregate => aggregate.DeleteQuestionnaire)
                .InitializesWith<DisableQuestionnaire>(aggregate => aggregate.DisableQuestionnaire)
                .InitializesWith<CloneQuestionnaire>(aggregate => aggregate.CloneQuestionnaire);

            this.Bind<User>().ToSelf();
            this.Bind<IPlainAggregateRootRepository<User>>().To<UserRepository>();

            CommandRegistry
                .Setup<User>()
                .InitializesWith<CreateUserCommand>(command => command.PublicKey, (command, aggregate) => aggregate.CreateUser(command.Email, command.IsLockedBySupervisor, command.IsLockedByHQ, command.Password, command.PublicKey, command.Roles, command.Supervisor, command.UserName, command.PersonName, command.PhoneNumber))
                .Handles<ChangeUserCommand>(command => command.PublicKey, (command, aggregate) => aggregate.ChangeUser(command.Email, command.IsLockedBySupervisor, command.IsLockedByHQ, command.PasswordHash, command.PersonName, command.PhoneNumber, command.UserId))
                .Handles<LockUserCommand>(command => command.PublicKey, (command, aggregate) => aggregate.Lock())
                .Handles<ArchiveUserCommad>(command => command.UserId, (command, aggregate) => aggregate.Archive())
                .Handles<UnarchiveUserCommand>(command => command.UserId, (command, aggregate) => aggregate.Unarchive())
                .Handles<UnarchiveUserAndUpdateCommand>(command => command.UserId, (command, aggregate) => aggregate.UnarchiveAndUpdate(command.PasswordHash, command.Email, command.PersonName, command.PhoneNumber))
                .Handles<LockUserBySupervisorCommand>(command => command.UserId, (command, aggregate) => aggregate.LockBySupervisor())
                .Handles<UnlockUserCommand>(command => command.PublicKey, (command, aggregate) => aggregate.Unlock())
                .Handles<UnlockUserBySupervisorCommand>(command => command.PublicKey, (command, aggregate) => aggregate.UnlockBySupervisor())
                .Handles<LinkUserToDevice>(command => command.Id, (command, aggregate) => aggregate.LinkUserToDevice(command));

            CommandRegistry
                .Setup<Interview>()
                .InitializesWith<CreateInterviewFromSynchronizationMetadata>(command => command.InterviewId, (command, aggregate) => aggregate.CreateInterviewFromSynchronizationMetadata(command.Id, command.UserId, command.QuestionnaireId, command.QuestionnaireVersion, command.InterviewStatus, command.FeaturedQuestionsMeta, command.Comments, command.RejectedDateTime, command.InterviewerAssignedDateTime, command.Valid, command.CreatedOnClient))
                .InitializesWith<SynchronizeInterviewEventsCommand>(command => command.InterviewId, (command, aggregate) => aggregate.SynchronizeInterviewEvents(command.UserId, command.QuestionnaireId, command.QuestionnaireVersion, command.InterviewStatus, command.SynchronizedEvents, command.CreatedOnClient))
                .InitializesWith<CreateInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.CreateInterview(command.QuestionnaireId, command.QuestionnaireVersion, command.SupervisorId, command.AnswersToFeaturedQuestions, command.AnswersTime, command.UserId))
                .InitializesWith<CreateInterviewCreatedOnClientCommand>(command => command.InterviewId, (command, aggregate) => aggregate.CreateInterviewCreatedOnClient(command.QuestionnaireId, command.QuestionnaireVersion, command.InterviewStatus, command.FeaturedQuestionsMeta, command.IsValid, command.UserId))
                .InitializesWith<CreateInterviewOnClientCommand>(command => command.InterviewId, (command, aggregate) => aggregate.CreateInterviewOnClient(command.QuestionnaireIdentity, command.SupervisorId, command.AnswersTime, command.UserId))
                .InitializesWith<CreateInterviewWithPreloadedData>(command => command.InterviewId, (command, aggregate) => aggregate.CreateInterviewWithPreloadedData(command))
                .InitializesWith<SynchronizeInterviewFromHeadquarters>(command => command.InterviewId, (command, aggregate) => aggregate.SynchronizeInterviewFromHeadquarters(command.Id, command.UserId, command.SupervisorId, command.InterviewDto, command.SynchronizationTime))
                
                .StatelessHandles<HardDeleteInterview>(command => command.InterviewId, (command, aggregate) => aggregate.HardDelete(command.UserId))

                .Handles<AnswerDateTimeQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerDateTimeQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Answer), config => config.ValidatedBy<InterviewAnswersCommandValidator>())
                .Handles<AnswerGeoLocationQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerGeoLocationQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Latitude, command.Longitude, command.Accuracy, command.Altitude, command.Timestamp), config => config.ValidatedBy<InterviewAnswersCommandValidator>())
                .Handles<AnswerMultipleOptionsLinkedQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerMultipleOptionsLinkedQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.SelectedRosterVectors), config => config.ValidatedBy<InterviewAnswersCommandValidator>())
                .Handles<AnswerMultipleOptionsQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerMultipleOptionsQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.SelectedValues), config => config.ValidatedBy<InterviewAnswersCommandValidator>())
                .Handles<AnswerYesNoQuestion>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerYesNoQuestion(command), config => config.ValidatedBy<InterviewAnswersCommandValidator>())
                .Handles<AnswerNumericIntegerQuestionCommand>(command => command.InterviewId, aggregate => aggregate.AnswerNumericIntegerQuestion, config => config.ValidatedBy<InterviewAnswersCommandValidator>())
                .Handles<AnswerNumericRealQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerNumericRealQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Answer), config => config.ValidatedBy<InterviewAnswersCommandValidator>())
                .Handles<AnswerPictureQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerPictureQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.PictureFileName), config => config.ValidatedBy<InterviewAnswersCommandValidator>())
                .Handles<AnswerQRBarcodeQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerQRBarcodeQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Answer), config => config.ValidatedBy<InterviewAnswersCommandValidator>())
                .Handles<AnswerSingleOptionLinkedQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerSingleOptionLinkedQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.SelectedRosterVector), config => config.ValidatedBy<InterviewAnswersCommandValidator>())
                .Handles<AnswerSingleOptionQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerSingleOptionQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.SelectedValue), config => config.ValidatedBy<InterviewAnswersCommandValidator>())
                .Handles<AnswerTextListQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerTextListQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Answers), config => config.ValidatedBy<InterviewAnswersCommandValidator>())
                .Handles<AnswerTextQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerTextQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Answer), config => config.ValidatedBy<InterviewAnswersCommandValidator>())
                .Handles<RemoveAnswerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.RemoveAnswer(command.QuestionId, command.RosterVector, command.UserId, command.RemoveTime))

                .Handles<ApproveInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Approve(command.UserId, command.Comment, command.ApproveTime))
                .Handles<AssignInterviewerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AssignInterviewer(command.UserId, command.InterviewerId, command.AssignTime))
                .Handles<AssignSupervisorCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AssignSupervisor(command.UserId, command.SupervisorId))
                .Handles<CancelInterviewByHqSynchronizationCommand>(command => command.InterviewId, (command, aggregate) => aggregate.CancelByHQSynchronization(command.UserId))
                .Handles<CommentAnswerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.CommentAnswer(command.UserId, command.QuestionId, command.RosterVector, command.CommentTime, command.Comment))
                .Handles<DeleteInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Delete(command.UserId))
                .Handles<HqApproveInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.HqApprove(command.UserId, command.Comment))
                .Handles<HqRejectInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.HqReject(command.UserId, command.Comment))
                .Handles<UnapproveByHeadquartersCommand>(command => command.InterviewId, (command, aggregate) => aggregate.UnapproveByHeadquarters(command.UserId, command.Comment))
                .Handles<MarkInterviewAsSentToHeadquarters>(command => command.InterviewId, (command, aggregate) => aggregate.MarkInterviewAsSentToHeadquarters(command.UserId))
                .Handles<MarkInterviewAsReceivedByInterviewer>(command => command.InterviewId, (command, aggregate) => aggregate.MarkInterviewAsReceivedByInterviwer(command.UserId))
                .Handles<ReevaluateSynchronizedInterview>(command => command.InterviewId, (command, aggregate) => aggregate.ReevaluateSynchronizedInterview())
                .Handles<RepeatLastInterviewStatus>(command => command.InterviewId, aggregate => aggregate.RepeatLastInterviewStatus)
                .Handles<RejectInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Reject(command.UserId, command.Comment, command.RejectTime))
                .Handles<RejectInterviewFromHeadquartersCommand>(command => command.InterviewId, (command, aggregate) => aggregate.RejectInterviewFromHeadquarters(command.UserId, command.SupervisorId, command.InterviewerId, command.InterviewDto, command.SynchronizationTime))
                .Handles<RemoveFlagFromAnswerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.RemoveFlagFromAnswer(command.UserId, command.QuestionId, command.RosterVector))
                .Handles<RestartInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Restart(command.UserId, command.Comment, command.RestartTime))
                .Handles<RestoreInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Restore(command.UserId))
                .Handles<SetFlagToAnswerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.SetFlagToAnswer(command.UserId, command.QuestionId, command.RosterVector))
                .Handles<SynchronizeInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.SynchronizeInterview(command.UserId, command.SynchronizedInterview))
                .Handles<SwitchTranslation>(command => command.InterviewId, aggregate => aggregate.SwitchTranslation);

            CommandRegistry.Configure<Interview, SynchronizeInterviewEventsCommand>(configuration => configuration.ValidatedBy<SurveyManagementInterviewCommandValidator>());
            CommandRegistry.Configure<Interview, CreateInterviewWithPreloadedData>(configuration => configuration.ValidatedBy<SurveyManagementInterviewCommandValidator>());
            CommandRegistry.Configure<Interview, CreateInterviewCommand>(configuration => configuration.ValidatedBy<SurveyManagementInterviewCommandValidator>());
            CommandRegistry.Configure<Interview, CreateInterviewOnClientCommand>(configuration => configuration.ValidatedBy<SurveyManagementInterviewCommandValidator>());
            CommandRegistry.Configure<Interview, CreateInterviewCreatedOnClientCommand>(configuration => configuration.ValidatedBy<SurveyManagementInterviewCommandValidator>());
            
            this.Bind<IAndroidPackageReader>().To<AndroidPackageReader>();
           
            this.Bind<IPreloadingTemplateService>().To<PreloadingTemplateService>().WithConstructorArgument("folderPath", this.currentFolderPath);
            this.Bind<IPreloadedDataRepository>().To<FilebasedPreloadedDataRepository>().WithConstructorArgument("folderPath", this.currentFolderPath);
            this.Bind<IPreloadedDataVerifier>().To<PreloadedDataVerifier>();
            this.Bind<IQuestionDataParser>().To<QuestionDataParser>();
            this.Bind<IPreloadedDataService>().To<PreloadedDataService>();

            this.Bind<IExportFileNameService>().To<ExportExportFileNameService>();

            //commented because auto registered somewhere 
            //this.Bind<IMetaDescriptionFactory>().To<MetaDescriptionFactory>();
            this.Bind<IRecordsAccessorFactory>().To<CsvRecordsAccessorFactory>();
            this.Bind<IInterviewSynchronizationDtoFactory>().To<InterviewSynchronizationDtoFactory>();
            this.Bind<IPreloadedDataServiceFactory>().To<PreloadedDataServiceFactory>();
            this.Bind<IBrokenInterviewPackagesViewFactory>().To<BrokenInterviewPackagesViewFactory>();
            this.Bind<ISynchronizationLogViewFactory>().To<SynchronizationLogViewFactory>();
            this.Bind<IInterviewsToDeleteFactory>().To<InterviewsToDeleteFactory>();
            this.Bind<Func<IInterviewsToDeleteFactory>>().ToMethod(context => () => context.Kernel.Get<IInterviewsToDeleteFactory>());
            this.Bind<IInterviewHistoryFactory>().To<InterviewHistoryFactory>();
            this.Bind<ISupervisorsViewFactory>().To<SupervisorsViewFactory>();
            this.Bind<IInterviewInformationFactory>().To<InterviewerInterviewsFactory>();
            this.Bind<IQuestionnaireRosterStructureFactory>().To<QuestionnaireRosterStructureFactory>();
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
            this.Bind<IQuestionnaireQuestionInfoFactory>().To<QuestionnaireQuestionInfoFactory>();
            this.Bind<ISpeedReportFactory>().To<SpeedReportFactory>();
            this.Bind<ISampleUploadViewFactory>().To<SampleUploadViewFactory>();
            this.Bind<ITakeNewInterviewViewFactory>().To<TakeNewInterviewViewFactory>();
            this.Bind<IAllUsersAndQuestionnairesFactory>().To<AllUsersAndQuestionnairesFactory>();
            this.Bind<IQuestionnairePreloadingDataViewFactory>().To<QuestionnairePreloadingDataViewFactory>();
            this.Bind<IInterviewTroubleshootFactory>().To<InterviewTroubleshootFactory>();
            this.Kernel.Bind<ITeamViewFactory>().To<TeamViewFactory>();
            this.Kernel.Bind<IUserListViewFactory>().To<UserListViewFactory>();
            this.Kernel.Bind<IUserViewFactory>().To<UserViewFactory>();
            this.Kernel.Bind<ITeamUsersAndQuestionnairesFactory>().To<TeamUsersAndQuestionnairesFactory>();
            this.Kernel.Bind<IInterviewDetailsViewFactory>().To<InterviewDetailsViewFactory>();
            this.Kernel.Bind<IInterviewSummaryViewFactory>().To<InterviewSummaryViewFactory>();
            this.Kernel.Bind<IInterviewersViewFactory>().To<InterviewersViewFactory>();
            this.Kernel.Bind<IChartStatisticsViewFactory>().To<ChartStatisticsViewFactory>();
            this.Kernel.Bind<IQuestionnaireBrowseViewFactory>().To<QuestionnaireBrowseViewFactory>();

            this.Bind<IInterviewImportDataParsingService>().To<InterviewImportDataParsingService>();

            this.Bind<IOldschoolChartStatisticsDataProvider>().To<OldschoolChartStatisticsDataProvider>();

            this.Bind<ISupervisorTeamsAndStatusesReport>().To<SupervisorTeamsAndStatusesReport>();
            this.Bind<IHeadquartersTeamsAndStatusesReport>().To<HeadquartersTeamsAndStatusesReport>();
            this.Bind<ISurveysAndStatusesReport>().To<SurveysAndStatusesReport>();
            this.Bind<IMapReport>().To<MapReport>();

            this.Unbind<ISupportedVersionProvider>();
            this.Bind<ISupportedVersionProvider>().To<SupportedVersionProvider>();

            this.Bind<ISyncProtocolVersionProvider>().To<SyncProtocolVersionProvider>().InSingletonScope();

            this.Bind(typeof(ITemporaryDataStorage<>)).To(typeof(FileTemporaryDataStorage<>)).WithConstructorArgument("rootPath", this.currentFolderPath);
            
            this.Bind<InterviewDetailsDataLoaderSettings>().ToConstant(this.interviewDetailsDataLoaderSettings);
            this.Bind<InterviewDetailsBackgroundSchedulerTask>().ToSelf();

            this.Bind<ITabletInformationService>().To<FileBasedTabletInformationService>().WithConstructorArgument("parentFolder", this.currentFolderPath);

            this.Bind<IPasswordHasher>().To<PasswordHasher>().InSingletonScope(); // external class which cannot be put to self-describing module because ninject is not portable


            //this.Kernel.RegisterDenormalizer<CumulativeChartDenormalizer>();
            //this.Kernel.RegisterDenormalizer<DummyEventHandler>();
            //this.Kernel.RegisterDenormalizer<InterviewReferencesDenormalizer>();
            //this.Kernel.RegisterDenormalizer<InterviewSummaryDenormalizer>();
            //this.Kernel.RegisterDenormalizer<MapReportDenormalizer>();
            //this.Kernel.RegisterDenormalizer<TabletDenormalizer>();
            //this.Kernel.RegisterDenormalizer<InterviewsExportDenormalizer>();

            this.Kernel.RegisterDenormalizer<InterviewEventHandlerFunctional>();
            this.Kernel.RegisterDenormalizer<StatusChangeHistoryDenormalizerFunctional>();
            this.Kernel.RegisterDenormalizer<InterviewExportedCommentariesDenormalizer>();
            this.Kernel.RegisterDenormalizer<InterviewStatusTimeSpanDenormalizer>();
            this.Kernel.RegisterDenormalizer<LinkedOptionsDenormalizer>();

            this.Kernel.Load(new QuartzNinjectModule());

            this.Bind<IInterviewPackagesService>().To<IncomingSyncPackagesService>();

            this.Bind<ReadSideSettings>().ToConstant(this.readSideSettings);
            this.Bind<ReadSideService>().ToSelf().InSingletonScope();
            this.Bind<IReadSideStatusService>().ToMethod(context => context.Kernel.Get<ReadSideService>());
            this.Bind<IReadSideAdministrationService>().ToMethod(context => context.Kernel.Get<ReadSideService>());
         
            this.Bind<IDeleteQuestionnaireService>().To<DeleteQuestionnaireService>().InSingletonScope();
            this.Bind<IDeleteSupervisorService>().To<DeleteSupervisorService>().InSingletonScope();
            this.Bind<IAtomicHealthCheck<EventStoreHealthCheckResult>>().To<EventStoreHealthChecker>();
            this.Bind<IAtomicHealthCheck<FolderPermissionCheckResult>>().To<FolderPermissionChecker>().WithConstructorArgument("folderPath", this.currentFolderPath);
            this.Bind<IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult>>().To<NumberOfUnhandledPackagesChecker>();
            this.Bind<IAtomicHealthCheck<ReadSideHealthCheckResult>>().To<ReadSideHealthChecker>();

            this.Bind<IHealthCheckService>().To<HealthCheckService>();
            this.Bind<ISubstitutionService>().To<SubstitutionService>();

            this.Bind<ITranslationStorage>().To<TranslationStorage>();
            this.Bind<IQuestionnaireTranslator>().To<QuestionnaireTranslator>();
            this.Bind<IQuestionnaireStorage>().To<QuestionnaireStorage>().InSingletonScope(); // has internal cache, so should be singleton

            this.Bind<IPlainInterviewFileStorage>().To<PlainInterviewFileStorage>()
                .InSingletonScope().WithConstructorArgument("rootDirectoryPath", this.currentFolderPath);

            this.Bind<IInterviewSynchronizationFileStorage>().To<InterviewSynchronizationFileStorage>()
                .InSingletonScope().WithConstructorArgument("rootDirectoryPath", this.currentFolderPath).WithConstructorArgument("syncDirectoryName", this.syncDirectoryName);

            this.Bind<IQuestionnaireAssemblyFileAccessor>().To<QuestionnaireAssemblyFileAccessor>()
                .InSingletonScope().WithConstructorArgument("folderPath", this.currentFolderPath).WithConstructorArgument("assemblyDirectoryName", this.questionnaireAssembliesDirectoryName);

            this.Bind<IInterviewExpressionStatePrototypeProvider>().To<InterviewExpressionStatePrototypeProvider>();
            this.Bind<IVariableToUIStringService>().To<VariableToUIStringService>();

            CommandRegistry.Configure<User, CreateUserCommand>(configuration => configuration.ValidatedBy<HeadquarterUserCommandValidator>());
            CommandRegistry.Configure<User, UnarchiveUserCommand>(configuration => configuration.ValidatedBy<HeadquarterUserCommandValidator>());
            CommandRegistry.Configure<User, UnarchiveUserAndUpdateCommand>(configuration => configuration.ValidatedBy<HeadquarterUserCommandValidator>());

            this.Bind<UserPreloadingSettings>().ToConstant(this.userPreloadingSettings);
            this.Bind<IUserBatchCreator>().To<UserBatchCreator>();

            this.Bind<IUserPreloadingVerifier>().To<UserPreloadingVerifier>().InSingletonScope();
            this.Bind<IUserPreloadingCleaner>().To<UserPreloadingCleaner>().InSingletonScope();

            this.Bind<SampleImportSettings>().ToConstant(sampleImportSettings);

            this.Bind<InterviewDataExportSettings>().ToConstant(this.interviewDataExportSettings);
            this.Bind<ExportSettings>().ToConstant(this.exportSettings);
            this.Bind<IFilebasedExportedDataAccessor>().To<FilebasedExportedDataAccessor>();

            this.Bind<IDdiMetadataAccessor>().To<DdiMetadataAccessor>();
         
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
        }
    }
}
