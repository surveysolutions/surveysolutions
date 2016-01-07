using System;
using Ninject;
using Ninject.Modules;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Factories;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DeleteQuestionnaireTemplate;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DeleteSupervisor;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck.Checks;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.TabletInformation;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Implementation.TemporaryDataStorage;
using WB.Core.SharedKernels.SurveyManagement.QuartzIntegration;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.DeleteQuestionnaireTemplate;
using WB.Core.SharedKernels.SurveyManagement.Services.DeleteSupervisor;
using WB.Core.SharedKernels.SurveyManagement.Services.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Supervisor;
using WB.Core.SharedKernels.SurveySolutions.Implementation.Services;
using WB.Core.SharedKernels.SurveySolutions.Services;
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
        private readonly bool isSupervisorFunctionsEnabled;
        private readonly int? interviewLimitCount;
        private readonly ReadSideSettings readSideSettings;
        private readonly string syncDirectoryName;
        private readonly string questionnaireAssembliesDirectoryName;

        public SurveyManagementSharedKernelModule(string currentFolderPath,
            Func<bool> isDebug, Version applicationBuildVersion,
            InterviewDetailsDataLoaderSettings interviewDetailsDataLoaderSettings,
            ReadSideSettings readSideSettings,
            bool isSupervisorFunctionsEnabled,
            int? interviewLimitCount = null,
            string syncDirectoryName = "SYNC",
            string questionnaireAssembliesFolder = "QuestionnaireAssemblies")
        {
            this.currentFolderPath = currentFolderPath;
            this.isDebug = isDebug;
            this.interviewDetailsDataLoaderSettings = interviewDetailsDataLoaderSettings;
            this.applicationBuildVersion = applicationBuildVersion;
            this.readSideSettings = readSideSettings;
            this.isSupervisorFunctionsEnabled = isSupervisorFunctionsEnabled;
            this.interviewLimitCount = interviewLimitCount;

            this.syncDirectoryName = syncDirectoryName;
            this.questionnaireAssembliesDirectoryName = questionnaireAssembliesFolder;
        }

        public override void Load()
        {
            this.Bind<InterviewPreconditionsServiceSettings>().ToConstant(new InterviewPreconditionsServiceSettings(interviewLimitCount));

            CommandRegistry
                .Setup<Questionnaire>()
                .ResolvesIdFrom<QuestionnaireCommand>(command => command.QuestionnaireId)
                .InitializesWith<ImportFromDesigner>(aggregate => aggregate.ImportFromDesigner, config => config.ValidatedBy<QuestionnaireNameValidator>())
                .InitializesWith<RegisterPlainQuestionnaire>(aggregate => aggregate.RegisterPlainQuestionnaire)
                .Handles<DeleteQuestionnaire>(aggregate => aggregate.DeleteQuestionnaire)
                .Handles<DisableQuestionnaire>(aggregate => aggregate.DisableQuestionnaire);

            CommandRegistry
                .Setup<User>()
                .InitializesWith<CreateUserCommand>(command => command.PublicKey, (command, aggregate) => aggregate.CreateUser(command.Email, command.IsLockedBySupervisor, command.IsLockedByHQ, command.Password, command.PublicKey, command.Roles, command.Supervisor, command.UserName, command.PersonName, command.PhoneNumber))
                .Handles<ChangeUserCommand>(command => command.PublicKey, (command, aggregate) => aggregate.ChangeUser(command.Email, command.IsLockedBySupervisor, command.IsLockedByHQ, command.PasswordHash, command.PersonName, command.PhoneNumber, command.UserId))
                .Handles<LockUserCommand>(command => command.PublicKey, (command, aggregate) => aggregate.Lock())
                .Handles<ArchiveUserCommad>(command => command.UserId, (command, aggregate) => aggregate.Archive())
                .Handles<UnarchiveUserCommand>(command => command.UserId, (command, aggregate) => aggregate.Unarchive())
                .Handles<UnarchiveUserAndUpdateCommand>(command => command.UserId, (command, aggregate) => aggregate.UnarchiveUserAndUpdate(command.PasswordHash, command.Email, command.PersonName, command.PhoneNumber))
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
                .InitializesWith<CreateInterviewOnClientCommand>(command => command.InterviewId, (command, aggregate) => aggregate.CreateInterviewOnClient(command.QuestionnaireId, command.QuestionnaireVersion, command.SupervisorId, command.AnswersTime, command.UserId))
                .InitializesWith<CreateInterviewWithPreloadedData>(command => command.InterviewId, (command, aggregate) => aggregate.CreateInterviewWithPreloadedData(command.QuestionnaireId, command.Version, command.PreloadedData, command.SupervisorId, command.AnswersTime, command.UserId))
                .InitializesWith<SynchronizeInterviewFromHeadquarters>(command => command.InterviewId, (command, aggregate) => aggregate.SynchronizeInterviewFromHeadquarters(command.Id, command.UserId, command.SupervisorId, command.InterviewDto, command.SynchronizationTime))
                .InitializesWith<CreateInterviewByPrefilledQuestions>(command => command.InterviewId, (command, aggregate) => aggregate.CreateInterviewByPrefilledQuestions(command.QuestionnaireIdentity, command.UserId, command.SupervisorId, command.InterviewerId, command.AnswersTime, command.AnswersOnPrefilledQuestions))
                .Handles<AnswerDateTimeQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerDateTimeQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Answer))
                .Handles<AnswerGeoLocationQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerGeoLocationQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Latitude, command.Longitude, command.Accuracy, command.Altitude, command.Timestamp))
                .Handles<AnswerMultipleOptionsLinkedQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerMultipleOptionsLinkedQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.SelectedRosterVectors))
                .Handles<AnswerMultipleOptionsQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerMultipleOptionsQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.SelectedValues))
                .Handles<AnswerYesNoQuestion>(command => command.InterviewId, aggregate => aggregate.AnswerYesNoQuestion)
                .Handles<AnswerNumericIntegerQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerNumericIntegerQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Answer))
                .Handles<AnswerNumericRealQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerNumericRealQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Answer))
                .Handles<AnswerPictureQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerPictureQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.PictureFileName))
                .Handles<AnswerQRBarcodeQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerQRBarcodeQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Answer))
                .Handles<AnswerSingleOptionLinkedQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerSingleOptionLinkedQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.SelectedRosterVector))
                .Handles<AnswerSingleOptionQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerSingleOptionQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.SelectedValue))
                .Handles<AnswerTextListQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerTextListQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Answers))
                .Handles<AnswerTextQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerTextQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Answer))
                .Handles<RemoveAnswerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.RemoveAnswer(command.QuestionId, command.RosterVector, command.UserId, command.RemoveTime))

                .Handles<ApproveInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Approve(command.UserId, command.Comment, command.ApproveTime))
                .Handles<AssignInterviewerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AssignInterviewer(command.UserId, command.InterviewerId, command.AssignTime))
                .Handles<AssignSupervisorCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AssignSupervisor(command.UserId, command.SupervisorId))
                .Handles<CancelInterviewByHqSynchronizationCommand>(command => command.InterviewId, (command, aggregate) => aggregate.CancelByHQSynchronization(command.UserId))
                .Handles<CommentAnswerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.CommentAnswer(command.UserId, command.QuestionId, command.RosterVector, command.CommentTime, command.Comment))
                .Handles<CompleteInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Complete(command.UserId, command.Comment, command.CompleteTime))
                .Handles<DeleteInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Delete(command.UserId))
                .Handles<HardDeleteInterview>(command => command.InterviewId, (command, aggregate) => aggregate.HardDelete(command.UserId))
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
                .Handles<SynchronizeInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.SynchronizeInterview(command.UserId, command.SynchronizedInterview));

            CommandRegistry.Configure<Interview, SynchronizeInterviewEventsCommand>(configuration => configuration.ValidatedBy<SurveyManagementInterviewCommandValidator>());
            CommandRegistry.Configure<Interview, CreateInterviewWithPreloadedData>(configuration => configuration.ValidatedBy<SurveyManagementInterviewCommandValidator>());
            CommandRegistry.Configure<Interview, CreateInterviewCommand>(configuration => configuration.ValidatedBy<SurveyManagementInterviewCommandValidator>());
            CommandRegistry.Configure<Interview, CreateInterviewOnClientCommand>(configuration => configuration.ValidatedBy<SurveyManagementInterviewCommandValidator>());
            CommandRegistry.Configure<Interview, CreateInterviewCreatedOnClientCommand>(configuration => configuration.ValidatedBy<SurveyManagementInterviewCommandValidator>());
            CommandRegistry.Configure<Interview, CreateInterviewByPrefilledQuestions>(configuration => configuration.ValidatedBy<SurveyManagementInterviewCommandValidator>());

            this.Bind<ISampleImportService>().To<SampleImportService>();
            this.Bind<Func<ISampleImportService>>().ToMethod(context => () => context.Kernel.Get<ISampleImportService>());
           
            //commented because auto registered somewhere 
            //this.Bind<IMetaDescriptionFactory>().To<MetaDescriptionFactory>();
            this.Bind<IPreloadingTemplateService>().To<PreloadingTemplateService>().WithConstructorArgument("folderPath", this.currentFolderPath);
            this.Bind<IPreloadedDataRepository>().To<FilebasedPreloadedDataRepository>().WithConstructorArgument("folderPath", this.currentFolderPath);
            this.Bind<IPreloadedDataVerifier>().To<PreloadedDataVerifier>();
            this.Bind<IRecordsAccessorFactory>().To<CsvRecordsAccessorFactory>();
            this.Bind<IQuestionDataParser>().To<QuestionDataParser>();
            this.Bind<IPreloadedDataService>().To<PreloadedDataService>();
            this.Bind<IInterviewSynchronizationDtoFactory>().To<InterviewSynchronizationDtoFactory>();
            this.Bind<IPreloadedDataServiceFactory>().To<PreloadedDataServiceFactory>();

            this.Bind<IOldschoolChartStatisticsDataProvider>().To<OldschoolChartStatisticsDataProvider>();

            this.Bind<ISupervisorTeamsAndStatusesReport>().To<SupervisorTeamsAndStatusesReport>();
            this.Bind<IHeadquartersTeamsAndStatusesReport>().To<HeadquartersTeamsAndStatusesReport>();
            this.Bind<ISurveysAndStatusesReport>().To<SurveysAndStatusesReport>();
            this.Bind<IMapReport>().To<MapReport>();

            this.Unbind<ISupportedVersionProvider>();
            this.Bind<ISupportedVersionProvider>()
                .ToConstant(new SupportedVersionProvider(this.isDebug, this.applicationBuildVersion));

            this.Bind<ISyncProtocolVersionProvider>().To<SyncProtocolVersionProvider>().InSingletonScope();

            this.Bind(typeof(ITemporaryDataStorage<>)).To(typeof(FileTemporaryDataStorage<>)).WithConstructorArgument("rootPath", this.currentFolderPath);

            this.Bind<IQuestionnaireCacheInitializer>().To<QuestionnaireCacheInitializer>();
            this.Bind<InterviewDetailsDataLoaderSettings>().ToConstant(this.interviewDetailsDataLoaderSettings);
            this.Bind<InterviewDetailsBackgroundSchedulerTask>().ToSelf();

            this.Bind<ITabletInformationService>().To<FileBasedTabletInformationService>().WithConstructorArgument("parentFolder", this.currentFolderPath);
            this.Bind<IReferenceInfoForLinkedQuestionsFactory>().To<ReferenceInfoForLinkedQuestionsFactory>();

            this.Bind<IPasswordHasher>().To<PasswordHasher>().InSingletonScope(); // external class which cannot be put to self-describing module because ninject is not portable

            this.Bind<ISynchronizationLogViewFactory>().To<SynchronizationLogViewFactory>();

            this.Kernel.RegisterDenormalizer<InterviewEventHandlerFunctional>();
            this.Kernel.RegisterDenormalizer<StatusChangeHistoryDenormalizerFunctional>();
            this.Kernel.RegisterDenormalizer<InterviewExportedCommentariesDenormalizer>();
            this.Kernel.RegisterDenormalizer<InterviewStatusTimeSpanDenormalizer>();

            this.Kernel.Load(new QuartzNinjectModule());

            if (isSupervisorFunctionsEnabled)
            {
                this.Kernel.RegisterDenormalizer<InterviewSynchronizationDenormalizer>();
                this.Kernel.RegisterDenormalizer<TabletDenormalizer>();
            }

            this.Bind<IBrokenSyncPackagesStorage>()
                .To<BrokenSyncPackagesStorage>();

            this.Bind<ISyncPackagesProcessor>()
                .To<SyncPackagesProcessor>()
                .InSingletonScope();

            this.Bind<IIncomingSyncPackagesQueue>()
              .To<IncomingSyncPackagesQueue>()
              .InSingletonScope();

            this.Bind<ReadSideSettings>().ToConstant(this.readSideSettings);
            this.Bind<ReadSideService>().ToSelf().InSingletonScope();
            this.Bind<IReadSideStatusService>().ToMethod(context => context.Kernel.Get<ReadSideService>());
            this.Bind<IReadSideAdministrationService>().ToMethod(context => context.Kernel.Get<ReadSideService>());

            this.Bind<IInterviewsToDeleteFactory>().To<InterviewsToDeleteFactory>();
            this.Bind<Func<IInterviewsToDeleteFactory>>().ToMethod(context => () => context.Kernel.Get<IInterviewsToDeleteFactory>());
            this.Bind<IDeleteQuestionnaireService>().To<DeleteQuestionnaireService>().InSingletonScope();
            this.Bind<IDeleteSupervisorService>().To<DeleteSupervisorService>().InSingletonScope();
            this.Bind<IInterviewHistoryFactory>().To<InterviewHistoryFactory>();

            this.Bind<IAtomicHealthCheck<EventStoreHealthCheckResult>>().To<EventStoreHealthChecker>();
            this.Bind<IAtomicHealthCheck<FolderPermissionCheckResult>>().To<FolderPermissionChecker>().WithConstructorArgument("folderPath", this.currentFolderPath);
            this.Bind<IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult>>().To<NumberOfUnhandledPackagesChecker>();
            this.Bind<IAtomicHealthCheck<ReadSideHealthCheckResult>>().To<ReadSideHealthChecker>();

            this.Bind<IHealthCheckService>().To<HealthCheckService>();

            this.Bind<ISupervisorsViewFactory>().To<SupervisorsViewFactory>();
            this.Bind<IInterviewInformationFactory>().To<InterviewerInterviewsFactory>();
            this.Bind<ISubstitutionService>().To<SubstitutionService>();

            
            this.Bind<IPlainQuestionnaireRepository>().To<PlainQuestionnaireRepositoryWithCache>().InSingletonScope(); // has internal cache, so should be singleton
            this.Bind<IQuestionnaireFactory>().To<QuestionnaireFactory>();
            this.Bind<IQuestionnaireRosterStructureFactory>().To<QuestionnaireRosterStructureFactory>();

            this.Bind<IPlainInterviewFileStorage>().To<PlainInterviewFileStorage>()
                .InSingletonScope().WithConstructorArgument("rootDirectoryPath", this.currentFolderPath);

            this.Bind<IInterviewSynchronizationFileStorage>().To<InterviewSynchronizationFileStorage>()
                .InSingletonScope().WithConstructorArgument("rootDirectoryPath", this.currentFolderPath).WithConstructorArgument("syncDirectoryName", this.syncDirectoryName);

            this.Bind<IQuestionnaireAssemblyFileAccessor>().To<QuestionnaireAssemblyFileAccessor>()
                .InSingletonScope().WithConstructorArgument("folderPath", this.currentFolderPath).WithConstructorArgument("assemblyDirectoryName", this.questionnaireAssembliesDirectoryName);

            this.Bind<IInterviewExpressionStatePrototypeProvider>().To<InterviewExpressionStatePrototypeProvider>();
            this.Bind<IInterviewExpressionStateUpgrader>().To<InterviewExpressionStateUpgrader>().InSingletonScope();
        }
    }
}
