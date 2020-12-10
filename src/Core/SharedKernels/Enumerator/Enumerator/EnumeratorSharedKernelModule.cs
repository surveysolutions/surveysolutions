﻿using System;
using System.Threading.Tasks;
using Ncqrs;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.HttpServices.Services;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Implementation.Services;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services.Implementation;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class EnumeratorSharedKernelModule : IModule, IInitModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<ILiteEventBus, LiteEventBus>();

            registry.Bind<IEntitiesListViewModelFactory, EntitiesListViewModelFactory>();
            registry.Bind<IDynamicTextViewModelFactory, DynamicTextViewModelFactory>();
            registry.Bind<ISubstitutionTextFactory, SubstitutionTextFactory>();
            registry.Bind<ISubstitutionService, SubstitutionService>(); //.InScope(ctx => BaseInterviewViewModel.CurrentInterviewScope); 
            registry.Bind<IVariableToUIStringService, VariableToUIStringService>();
            registry.Bind<IOptionsRepository, OptionsRepository>();
            registry.Bind<IQuestionOptionsRepository, QuestionOptionsRepository>();
            registry.Bind<IInterviewTreeBuilder, InterviewTreeBuilder>();
            registry.BindAsSingleton<IInterviewExpressionStateUpgrader, InterviewExpressionStateUpgrader>();
            registry.Bind<IInterviewExpressionStatePrototypeProvider, InterviewExpressionStatePrototypeProvider>();
            registry.BindAsSingleton<IFriendlyErrorMessageService, FriendlyErrorMessageService>();
            registry.Bind<ICompositeCollectionInflationService, CompositeCollectionInflationService>();
            registry.BindAsSingleton<ILastCompletionComments, LastCompletionComments>();
            registry.Bind<IInterviewsRemover, Implementation.Services.Synchronization.Steps.InterviewsRemover>();
            registry.Bind<ICompanyLogoSynchronizer, CompanyLogoSynchronizer>();
            registry.Bind<IAttachmentsCleanupService, AttachmentsCleanupService>();
            
            registry.Bind<NavigationState>();
            registry.Bind<AnswerNotifier>();

            // offline sync
            registry.Bind<IOfflineSyncClient, OfflineSyncClient>();
            registry.BindAsSingleton<IPayloadSerializer, PayloadSerializer>();
            registry.BindAsSingleton<INearbyConnection, NearbyConnection>();

            registry.BindAsSingleton<ISynchronizationCompleteSource, SynchronizationCompleteSource>();

            registry.Bind<IAudioFileStorage, InterviewerAudioFileStorage>();
            registry.Bind<IImageFileStorage, InterviewerImageFileStorage>();
            registry.Bind<IAudioAuditFileStorage, InterviewerAudioAuditFileStorage>();
            registry.BindAsSingleton<IAuditLogService, EnumeratorAuditLogService>();
            registry.Bind<IServiceProvider, MvxServiceProvider>();
            registry.Bind<IMigrationRunner, MigrationRunner>();

            registry.BindAsSingleton<IViewModelEventRegistry, ViewModelEventRegistry>();
            
            registry.Bind<ILiteEventBus, LiteEventBus>();
            registry.Bind<IAsyncEventDispatcher, AsyncEventDispatcher>();
            registry.BindAsSingleton<IAsyncEventQueue, AsyncEventQueue>();

            RegisterViewModels(registry);
        }

        private static void RegisterViewModels(IIocRegistry registry)
        {
            registry.Bind<GroupNavigationViewModel>();
            registry.Bind<StartInterviewViewModel>();

            registry.Bind<AnsweringViewModel>();
            registry.Bind<AudioDialogViewModel>();
            registry.Bind<VibrationViewModel>();
            registry.Bind<AudioDialogViewModel>();
            registry.Bind<BreadCrumbItemViewModel>();
            registry.Bind<BreadCrumbsViewModel>();
            registry.Bind<CompleteInterviewViewModel>();
            registry.Bind<CoverInterviewViewModel>();
            registry.Bind<DynamicTextViewModel>();
            registry.Bind<EntityWithErrorsViewModel>();
            registry.Bind<EntityWithCommentsViewModel>();
            registry.Bind<EnumerationStageViewModel>();
            registry.Bind<ReadOnlyQuestionViewModel>();
            registry.Bind<SideBarCompleteSectionViewModel>();
            registry.Bind<SideBarOverviewViewModel>();
            registry.Bind<OverviewViewModel>();
            registry.Bind<SideBarCoverSectionViewModel>();
            registry.Bind<SideBarSectionViewModel>();
            registry.Bind<SideBarSectionsViewModel>();
            registry.Bind<StaticTextStateViewModel>();
            registry.Bind<StaticTextViewModel>();
            registry.Bind<VariableViewModel>();
            registry.Bind<CoverStateViewModel>();
            registry.Bind<GroupStateViewModel>();
            registry.Bind<GroupViewModel>();
            registry.Bind<InterviewStateViewModel>();
            registry.Bind<RosterViewModel>();

            // questions
            registry.Bind<AreaQuestionViewModel>();
            registry.Bind<AudioQuestionViewModel>();
            registry.Bind<CascadingSingleOptionQuestionViewModel>();
            registry.Bind<DateTimeQuestionViewModel>();
            registry.Bind<FilteredSingleOptionQuestionViewModel>();
            registry.Bind<GpsCoordinatesQuestionViewModel>();
            registry.Bind<MultimediaQuestionViewModel>();
            registry.Bind<CategoricalMultiLinkedToListViewModel>();
            registry.Bind<CategoricalMultiOptionViewModel<int>>();
            registry.Bind<CategoricalMultiOptionViewModel<RosterVector>>();
            registry.Bind<CategoricalMultiLinkedToQuestionViewModel>();
            registry.Bind<CategoricalMultiLinkedToRosterTitleViewModel>();
            registry.Bind<CategoricalMultiViewModel>();
            registry.Bind<CategoricalMultiComboboxViewModel>();
            registry.Bind<QRBarcodeQuestionViewModel>();
            registry.Bind<RealQuestionViewModel>();
            registry.Bind<IntegerQuestionViewModel>();
            registry.Bind<SingleOptionLinkedQuestionOptionViewModel>();
            registry.Bind<SingleOptionLinkedQuestionViewModel>();
            registry.Bind<SingleOptionLinkedToListQuestionViewModel>();
            registry.Bind<SingleOptionLinkedQuestionOptionViewModel>();
            registry.Bind<SingleOptionQuestionViewModel>();
            registry.Bind<SingleOptionRosterLinkedQuestionViewModel>();
            registry.Bind<CategoricalComboboxAutocompleteViewModel>();
            registry.Bind<TextListAddNewItemViewModel>();
            registry.Bind<TextListQuestionViewModel>();
            registry.Bind<TextQuestionViewModel>();
            registry.Bind<TimestampQuestionViewModel>();
            registry.Bind<CategoricalYesNoOptionViewModel>();
            registry.Bind<CategoricalYesNoViewModel>();
            
            // question state
            registry.Bind<AnswersRemovedNotifier>();
            registry.Bind<AttachmentViewModel>();
            registry.Bind<CommentsViewModel>();
            registry.Bind<EnablementViewModel>();
            registry.Bind<ErrorMessagesViewModel>();
            registry.Bind<FilteredOptionsViewModel>();
            registry.Bind<SpecialValuesViewModel>();
            registry.Bind<QuestionHeaderViewModel>();
            registry.Bind<QuestionInstructionViewModel>();
            registry.Bind<ValidityViewModel>();
            registry.Bind<WarningsViewModel>();
            registry.Bind<ErrorMessageViewModel>();
            registry.BindGeneric(typeof(QuestionStateViewModel<>));
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            var clock = serviceLocator.GetInstance<IClock>();
            CommandRegistry
                .Setup<StatefulInterview>()
                .InitializesWith<CreateInterviewFromSynchronizationMetadata>(command => command.InterviewId, (command, aggregate) => aggregate.CreateInterviewFromSynchronizationMetadata(command.Id, command.UserId, command.QuestionnaireId, command.QuestionnaireVersion, command.InterviewStatus, command.FeaturedQuestionsMeta, command.Comments, command.RejectedDateTime, command.InterviewerAssignedDateTime, command.Valid, command.CreatedOnClient, command.OriginDate))
                .InitializesWith<CreateInterview>(command => command.InterviewId, (command, aggregate) => aggregate.CreateInterview(command))
                .InitializesWith<SynchronizeInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Synchronize(command))
                .InitializesWith<CreateInterviewFromSnapshotCommand>(command => command.InterviewId, (command, aggregate) => aggregate.CreateInterviewFromSnapshot(command))
                .Handles<AnswerDateTimeQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerDateTimeQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Answer))
                .Handles<AnswerGeoLocationQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerGeoLocationQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Latitude, command.Longitude, command.Accuracy, command.Altitude, command.Timestamp))
                .Handles<AnswerMultipleOptionsLinkedQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerMultipleOptionsLinkedQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.SelectedRosterVectors))
                .Handles<AnswerMultipleOptionsQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerMultipleOptionsQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.SelectedValues))
                .Handles<AnswerYesNoQuestion>(command => command.InterviewId, aggregate => aggregate.AnswerYesNoQuestion)
                .Handles<AnswerNumericIntegerQuestionCommand>(command => command.InterviewId, aggregate => aggregate.AnswerNumericIntegerQuestion)
                .Handles<AnswerNumericRealQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerNumericRealQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Answer))
                .Handles<AnswerPictureQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerPictureQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.PictureFileName))
                .Handles<AnswerQRBarcodeQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerQRBarcodeQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Answer))
                .Handles<AnswerSingleOptionLinkedQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerSingleOptionLinkedQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.SelectedRosterVector))
                .Handles<AnswerSingleOptionQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerSingleOptionQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.SelectedValue))
                .Handles<AnswerTextListQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerTextListQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Answers))
                .Handles<AnswerTextQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerTextQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Answer))
                .Handles<AnswerAudioQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerAudioQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.FileName, command.Length))
                .Handles<RemoveAnswerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.RemoveAnswer(command.QuestionId, command.RosterVector, command.UserId, command.OriginDate))
                .Handles<AnswerGeographyQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerAreaQuestion(command))
                .Handles<ApproveInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Approve(command.UserId, command.Comment, command.OriginDate))
                .Handles<AssignInterviewerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AssignInterviewer(command.UserId, command.InterviewerId, command.OriginDate))
                .Handles<AssignSupervisorCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AssignSupervisor(command.UserId, command.SupervisorId, command.OriginDate))
                .Handles<AssignResponsibleCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AssignResponsible(command))
                .Handles<CommentAnswerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.CommentAnswer(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Comment))
                .Handles<ResolveCommentAnswerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.ResolveComment(command))
                .Handles<CompleteInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Complete(command.UserId, command.Comment, command.OriginDate))
                .Handles<DeleteInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Delete(command.UserId, command.OriginDate))
                .Handles<HardDeleteInterview>(command => command.InterviewId, (command, aggregate) => aggregate.HardDelete(command.UserId, command.OriginDate))
                .Handles<HqApproveInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.HqApprove(command.UserId, command.Comment, command.OriginDate))
                .Handles<HqRejectInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.HqReject(command.UserId, command.Comment, command.OriginDate))
                .Handles<UnapproveByHeadquartersCommand>(command => command.InterviewId, (command, aggregate) => aggregate.UnapproveByHeadquarters(command.UserId, command.Comment, command.OriginDate))
                .Handles<ReevaluateInterview>(command => command.InterviewId, (command, aggregate) => aggregate.ReevaluateInterview(command.UserId, clock.DateTimeOffsetNow()))
                .Handles<RejectInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Reject(command.UserId, command.Comment, command.OriginDate))
                .Handles<RejectInterviewToInterviewerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.RejectToInterviewer(command.UserId, command.InterviewerId, command.Comment, command.OriginDate))
                //.Handles<RejectInterviewFromHeadquartersCommand>(command => command.InterviewId, (command, aggregate) => aggregate.RejectInterviewFromHeadquarters(command.UserId, command.SupervisorId, command.InterviewerId, command.InterviewDto, command.SynchronizationTime))
                .Handles<RestartInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Restart(command.UserId, command.Comment, command.RestartTime))
                .Handles<RestoreInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Restore(command.UserId, command.OriginDate))
                .Handles<SwitchTranslation>(command => command.InterviewId, aggregate => aggregate.SwitchTranslation)
                .Handles<ResumeInterviewCommand>(command => command.InterviewId, aggregate => aggregate.Resume)
                .Handles<PauseInterviewCommand>(command => command.InterviewId, aggregate => aggregate.Pause);

            return Task.CompletedTask;
        }
    }
}
