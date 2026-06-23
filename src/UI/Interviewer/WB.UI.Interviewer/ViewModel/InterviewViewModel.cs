using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
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
        private readonly IAuditLogService auditLogService;
        private readonly IAudioAuditService audioAuditService;
        private readonly IUserInteractionService userInteractionService;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IJsonAllTypesSerializer serializer;
        private readonly IMvxMainThreadAsyncDispatcher asyncDispatcher;
        private bool isAuditStarting;
        private bool isViewVisible;
        private bool isAudioRecording;
        private Guid? currentRecordingKey;
        // Sentinel recording key used when the whole-interview audio audit flag is on. Real group
        // ids are never Guid.Empty, so there is no collision with the per-group scope keys.
        private static readonly Guid WholeInterviewRecordingKey = Guid.Empty;
        private readonly SemaphoreSlim audioRecordingLock = new SemaphoreSlim(1, 1);
        private readonly CancellationTokenSource audioRecordingCancellation = new CancellationTokenSource();
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
            IMvxMainThreadAsyncDispatcher asyncDispatcher)
            : base(questionnaireRepository, interviewRepository, sectionsViewModel,
                breadCrumbsViewModel, navigationState, answerNotifier, groupState, interviewState, coverState,
                principal, viewModelNavigationService,
                interviewViewModelFactory, commandService, vibrationViewModel, enumeratorSettings)
        {
            this.auditLogService = auditLogService;
            this.audioAuditService = audioAuditService;
            this.userInteractionService = userInteractionService;
            this.logger = logger;
            this.interviewViewRepository = interviewViewRepository;
            this.serializer = serializer;
            this.asyncDispatcher = asyncDispatcher;

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
                var interviewId = Guid.Parse(InterviewId);
                var interview = interviewRepository.Get(this.InterviewId);
                if (interview == null) return;

                await commandService.ExecuteAsync(new ResumeInterviewCommand(interviewId,
                    Principal.CurrentUserIdentity.UserId, AgentDeviceType.Tablet));

                this.isViewVisible = true;

                await this.EvaluateAudioRecordingAsync(interviewId, this.audioRecordingCancellation.Token);

                auditLogService.Write(new OpenInterviewAuditLogEntity(interviewId, interviewKey?.ToString(),
                    assignmentId));
                base.ViewAppeared();
            });
        }

        private async Task StartAudioRecordingWithPermissionHandlingAsync(Guid interviewId)
        {
            await asyncDispatcher.ExecuteOnMainThreadAsync(async () =>
            {
                isAuditStarting = true;
                try
                {
                    await audioAuditService.StartAudioRecordingAsync(interviewId).ConfigureAwait(false);
                }
                catch (MissingPermissionsException missingPermissionsException)
                {
                    this.logger.Info("Audio audit failed to start.", exception: missingPermissionsException);
                    await this.ViewModelNavigationService.NavigateToDashboardAsync(this.InterviewId)
                        .ConfigureAwait(false);

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
                    await this.ViewModelNavigationService.NavigateToDashboardAsync(this.InterviewId)
                        .ConfigureAwait(false);
                    this.userInteractionService.ShowToast(exc.Message);
                }
                finally
                {
                    isAuditStarting = false;
                }
            });
        }

        private void OnScreenChanged(ScreenChangedEventArgs eventArgs)
        {
            if (!this.isViewVisible || this.InterviewId == null)
                return;

            var interviewId = Guid.Parse(this.InterviewId);
            var cancellationToken = this.audioRecordingCancellation.Token;
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
        /// Single entry point that converges the tablet audio recording to the desired state:
        /// records the whole interview when the audio audit flag is on, otherwise records only the
        /// groups included in the audio audit scope, otherwise records nothing. On a group switch the
        /// current recording is stopped and rerun for the new applicable group, reusing the same
        /// start/stop paths as the whole-interview audio audit.
        /// </summary>
        private async Task EvaluateAudioRecordingAsync(Guid interviewId, CancellationToken cancellationToken)
        {
            await this.audioRecordingLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                var targetKey = this.GetTargetRecordingKey();

                // Already in the desired state.
                if (this.isAudioRecording && this.currentRecordingKey == targetKey)
                    return;

                // Stop the current recording when the target changed (group switch or leaving scope),
                // so each applicable group is captured as its own audio file, like the general audio audit.
                if (this.isAudioRecording)
                {
                    this.isAudioRecording = false;
                    this.currentRecordingKey = null;
                    audioAuditService.StopAudioRecording(interviewId);
                }

                // Start recording for the new target.
                if (targetKey != null && !this.isAuditStarting)
                {
                    await this.StartAudioRecordingWithPermissionHandlingAsync(interviewId).ConfigureAwait(false);
                    this.isAudioRecording = true;
                    this.currentRecordingKey = targetKey;
                }
            }
            finally
            {
                this.audioRecordingLock.Release();
            }
        }

        // Decides what should currently be recorded: the audio audit flag takes precedence (whole
        // interview), then the audio audit scope (applicable group), otherwise nothing.
        private Guid? GetTargetRecordingKey()
        {
            if (!this.isViewVisible)
                return null;

            if (IsAudioRecordingEnabled == true)
                return WholeInterviewRecordingKey;

            var interview = interviewRepository.Get(this.InterviewId);
            if (interview == null)
                return null;

            var scope = interview.GetAudioAuditScope();
            if (scope == null || scope.Length == 0)
                return null;

            var currentGroup = this.NavigationState.CurrentGroup;
            return interview.ShouldRecordAudioForGroup(currentGroup) ? currentGroup?.Id : null;
        }

        // Teardown stop used when the interview view disappears. Kept lock-free and synchronous so it
        // cannot deadlock the UI thread against an in-flight start that awaits the main thread.
        private void StopAudioRecording(Guid interviewId)
        {
            if (this.isAudioRecording)
            {
                this.isAudioRecording = false;
                this.currentRecordingKey = null;
                audioAuditService.StopAudioRecording(interviewId);
            }
        }
        
        public override void ViewDisappearing()
        {
            this.isViewVisible = false;

            if (InterviewId != null && Principal.IsAuthenticated)
            {
                var interview = interviewRepository.Get(this.InterviewId);
                if (interview != null)
                {
                    var interviewId = interview.Id;

                    var pause = new PauseInterviewCommand(interviewId, Principal.CurrentUserIdentity.UserId);
                    commandService.Execute(pause);

                    auditLogService.Write(new CloseInterviewAuditLogEntity(interviewId, interviewKey?.ToString()));

                    this.StopAudioRecording(interviewId);
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
            this.audioRecordingCancellation.Cancel();
            base.Dispose();
        }
    }
}
