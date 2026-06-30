using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.Views;
using Xamarin.Essentials;

namespace WB.UI.Interviewer.ViewModel
{
    public class InterviewViewModel : BaseInterviewViewModel
    {
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IAuditLogService auditLogService;
        private readonly IAudioAuditService audioAuditService;
        private readonly IUserInteractionService userInteractionService;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IJsonAllTypesSerializer serializer;
        private readonly IMvxMainThreadAsyncDispatcher asyncDispatcher;
        private readonly IAudioAuditRecordingExecutor audioRecordingExecutor;
        private readonly ILogger logger;

        public InterviewViewModel(
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            SideBarSectionsViewModel sectionsViewModel,
            BreadCrumbsViewModel breadCrumbsViewModel,
            NavigationState navigationState,
            AnswerNotifier answerNotifier,
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            GroupStateViewModel groupState,
            InterviewStateViewModel interviewState,
            CoverStateViewModel coverState,
            IInterviewViewModelFactory interviewViewModelFactory,
            ICommandService commandService,
            VibrationViewModel vibrationViewModel,
            IEnumeratorSettings enumeratorSettings,
            IAuditLogService auditLogService,
            IAudioAuditService audioAuditService,
            IUserInteractionService userInteractionService,
            ILogger logger,
            IPlainStorage<InterviewView> interviewViewRepository,
            IJsonAllTypesSerializer serializer,
            IMvxMainThreadAsyncDispatcher asyncDispatcher,
            IAudioAuditRecordingExecutor audioRecordingExecutor)
            : base(questionnaireRepository, interviewRepository, sectionsViewModel,
                breadCrumbsViewModel, navigationState, answerNotifier, groupState, interviewState, coverState,
                principal, viewModelNavigationService,
                interviewViewModelFactory, commandService, vibrationViewModel, enumeratorSettings)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.auditLogService = auditLogService;
            this.audioAuditService = audioAuditService;
            this.userInteractionService = userInteractionService;
            this.logger = logger;
            this.interviewViewRepository = interviewViewRepository;
            this.serializer = serializer;
            this.asyncDispatcher = asyncDispatcher;
            this.audioRecordingExecutor = audioRecordingExecutor;

            this.NavigationState.ScreenChanged += this.OnScreenChanged;
        }

        public override IMvxCommand ReloadCommand => new MvxAsyncCommand(async () =>
            await this.ViewModelNavigationService.NavigateToInterviewAsync(this.InterviewId,
                this.NavigationState.CurrentNavigationIdentity));

        public IMvxCommand NavigateToMapsCommand =>
            new MvxAsyncCommand(() => this.ViewModelNavigationService.NavigateToAsync<MapsViewModel>());

        public override async Task NavigateBack()
        {
            await this.ViewModelNavigationService.NavigateFromInterviewAsync(this.InterviewId);
            this.Dispose();
        }

        protected override NavigationIdentity GetDefaultScreenToNavigate(IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            if (HasNotEmptyNoteFromSupervior || HasCommentsFromSupervior || HasPrefilledQuestions)
                return NavigationIdentity.CreateForCoverScreen();

            return base.GetDefaultScreenToNavigate(interview, questionnaire);
        }

        protected override BaseViewModel UpdateCurrentScreenViewModel(ScreenChangedEventArgs eventArgs)
        {
            switch (this.NavigationState.CurrentScreenType)
            {
                case ScreenType.Complete:
                    var completeInterviewViewModel =
                        this.interviewViewModelFactory.GetNew<InterviewerCompleteInterviewViewModel>();
                    completeInterviewViewModel.PropertyChanged += ChangeInterviewStatus;
                    completeInterviewViewModel.Configure(this.InterviewId, this.NavigationState);
                    return completeInterviewViewModel;
                default:
                    return base.UpdateCurrentScreenViewModel(eventArgs);
            }
        }

        private void ChangeInterviewStatus(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(InterviewerCompleteInterviewViewModel.IsLoading))
                return;
            
            var completeInterviewViewModel = (InterviewerCompleteInterviewViewModel)sender;
            if (!completeInterviewViewModel.IsLoading)
            {
                completeInterviewViewModel.PropertyChanged -= ChangeInterviewStatus;
                Status = completeInterviewViewModel.CompleteStatus;
            }
        }

        public override void ViewAppeared()
        {
            if (!this.Principal.IsAuthenticated)
            {
                this.ViewModelNavigationService.NavigateToLoginAsync().WaitAndUnwrapException();
                return;
            }

            Task.Run(async () =>
            {
                try
                {
                    var interviewId = Guid.Parse(InterviewId);
                    var interview = interviewRepository.Get(this.InterviewId);
                    if (interview == null) return;

                    await commandService.ExecuteAsync(new ResumeInterviewCommand(interviewId,
                        Principal.CurrentUserIdentity.UserId, AgentDeviceType.Tablet));

                    this.audioRecordingExecutor.IsViewVisible = true;

                    await this.EvaluateAudioRecordingAsync(interviewId, this.audioRecordingExecutor.CancellationToken);

                    auditLogService.Write(new OpenInterviewAuditLogEntity(interviewId, interviewKey?.ToString(),
                        assignmentId));
                    base.ViewAppeared();
                }
                catch (OperationCanceledException)
                {
                    // ViewModel is being disposed; nothing to do.
                }
                catch (Exception exc)
                {
                    this.logger.Warn("Audio audit evaluation failed on view appeared.", exception: exc);
                }
            });
        }

        private async Task<bool> StartAudioRecordingWithPermissionHandlingAsync(Guid interviewId)
        {
            var started = false;
            await asyncDispatcher.ExecuteOnMainThreadAsync(async () =>
            {
                if (!this.audioRecordingExecutor.IsViewVisible)
                    return;
                try
                {
                    await audioAuditService.StartAudioRecordingAsync(interviewId);
                    started = true;
                }
                catch (MissingPermissionsException missingPermissionsException)
                {
                    this.logger.Info("Audio audit failed to start.", exception: missingPermissionsException);
                    await this.ViewModelNavigationService.NavigateToDashboardAsync(this.InterviewId);

                    if (missingPermissionsException.PermissionType == typeof(Permissions.Microphone))
                    {
                        this.userInteractionService.ShowToast(UIResources.MissingPermissions_Microphone);
                    }
                    else if (missingPermissionsException.PermissionType == typeof(Permissions.StorageWrite))
                    {
                        this.userInteractionService.ShowToast(UIResources.MissingPermissions_Storage);
                    }
                    else
                    {
                        this.userInteractionService.ShowToast(missingPermissionsException.Message);
                    }
                }
                catch (Exception exc)
                {
                    logger.Warn("Audio audit failed to start.", exception: exc);
                    await this.ViewModelNavigationService.NavigateToDashboardAsync(this.InterviewId);
                    this.userInteractionService.ShowToast(exc.Message);
                }
            });

            return started;
        }

        private void OnScreenChanged(ScreenChangedEventArgs eventArgs)
        {
            if (!this.audioRecordingExecutor.IsViewVisible || this.InterviewId == null)
                return;

            var interviewId = Guid.Parse(this.InterviewId);
            var cancellationToken = this.audioRecordingExecutor.CancellationToken;
            Task.Run(async () =>
            {
                try
                {
                    await this.EvaluateAudioRecordingAsync(interviewId, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // ViewModel is being disposed; nothing to do.
                }
                catch (Exception exc)
                {
                    this.logger.Warn("Audio audit evaluation failed.", exception: exc);
                }
            });
        }

        /// <summary>
        /// Converges the tablet audio recording to the desired state via the dedicated
        /// <see cref="IAudioAuditRecordingExecutor"/>: records the whole interview when the audio audit
        /// flag is on, otherwise records only the groups included in the audio audit scope, otherwise
        /// records nothing. The view model only supplies the policy — which target is desired
        /// (<see cref="GetTargetRecording"/>) and how to start a recording with permission handling
        /// (<see cref="StartAudioRecordingWithPermissionHandlingAsync"/>) — while the executor owns the
        /// recording lock, in-flight start guard and state.
        /// </summary>
        private Task EvaluateAudioRecordingAsync(Guid interviewId, CancellationToken cancellationToken) =>
            this.audioRecordingExecutor.EvaluateAsync(interviewId, this.GetTargetRecording,
                this.StartAudioRecordingWithPermissionHandlingAsync, cancellationToken);

        // Decides what should currently be recorded: the audio audit flag takes precedence (whole
        // interview), then the audio audit scope (applicable group), otherwise nothing. Visibility is
        // handled by the executor before this is called.
        private RecordingTarget GetTargetRecording()
        {
            if (IsAudioRecordingEnabled == true)
                return RecordingTarget.WholeInterview;

            var interview = interviewRepository.Get(this.InterviewId);
            if (interview == null)
                return RecordingTarget.None;

            var scope = interview.GetAudioAuditScope();
            if (scope == null || scope.Length == 0)
                return RecordingTarget.None;

            // The currently navigated scope entity: a regular group/section/roster, or the cover page
            // section when the cover screen is shown (CoverInterviewViewModel). The cover screen has no
            // CurrentGroup, so resolve its section id from the questionnaire to honour a scope that
            // includes the cover page.
            var currentScopeEntityId = this.GetCurrentScopeEntityId(interview);
            // Audio Audit is disabled (the whole-interview flag is handled above) and the scope is
            // non-empty: record only while the currently navigated entity is itself listed in the
            // scope. Scope selection is explicit and is not inherited.
            if (currentScopeEntityId == null || Array.IndexOf(scope, currentScopeEntityId.Value) < 0)
                return RecordingTarget.None;

            return RecordingTarget.Group(currentScopeEntityId.Value);
        }

        // Resolves the id of the entity the interviewer is currently on for scope-membership purposes.
        // On the cover screen there is no CurrentGroup, so the cover page section id is used so that the
        // cover page can be part of the audio audit scope.
        private Guid? GetCurrentScopeEntityId(IStatefulInterview interview)
        {
            if (this.NavigationState.CurrentScreenType == ScreenType.Cover)
            {
                var questionnaire = this.questionnaireRepository.GetQuestionnaire(
                    interview.QuestionnaireIdentity, interview.Language);

                if (questionnaire is { IsCoverPageSupported: true })
                    return questionnaire.CoverPageSectionId;

                return null;
            }

            return this.NavigationState.CurrentGroup?.Id;
        }

        // Fire-and-forget wrapper used during teardown so unobserved exceptions are logged.
        private async Task StopAudioRecordingSafelyAsync(Guid interviewId)
        {
            try
            {
                await this.audioRecordingExecutor.StopAsync(interviewId).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                this.logger.Warn("Audio audit failed to stop on view disappearing.", exception: exc);
            }
        }
        
        public override void ViewDisappearing()
        {
            this.audioRecordingExecutor.IsViewVisible = false;

            if (InterviewId != null && Principal.IsAuthenticated)
            {
                var interview = interviewRepository.Get(this.InterviewId);
                if (interview != null)
                {
                    var interviewId = interview.Id;

                    var pause = new PauseInterviewCommand(interviewId, Principal.CurrentUserIdentity.UserId);
                    commandService.Execute(pause);

                    auditLogService.Write(new CloseInterviewAuditLogEntity(interviewId, interviewKey?.ToString()));

                    // IsViewVisible is already false. Stop any active recording through the executor's
                    // recording lock so the state mutation stays atomic with EvaluateAudioRecordingAsync.
                    // Fire-and-forget: awaiting the lock yields back to the UI thread instead of blocking it,
                    // so it cannot deadlock against an in-flight start, and teardown still completes after
                    // Dispose. The stop path touches neither interviewRepository nor any field disposed by
                    // this view model.
                    _ = this.StopAudioRecordingSafelyAsync(interviewId);
                }
            }

            var interviewView = this.interviewViewRepository.GetById(InterviewId);
            if (interviewView != null)
            {
                if (this.NavigationState.CurrentGroup != null)
                {
                    interviewView.LastVisitedSectionId = this.serializer.Serialize(this.NavigationState.CurrentGroup);
                }

                interviewView.LastVisitedScreenType = this.NavigationState.CurrentScreenType;
                this.interviewViewRepository.Store(interviewView);
            }

            base.ViewDisappearing();
        }

        public override void Dispose()
        {
            this.NavigationState.ScreenChanged -= this.OnScreenChanged;

            this.audioRecordingExecutor.Cancel();
            this.audioRecordingExecutor.Dispose();

            base.Dispose();
        }

    }
}
