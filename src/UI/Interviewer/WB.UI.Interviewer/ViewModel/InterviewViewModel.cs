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
        private RecordingTarget currentRecordingTarget = RecordingTarget.None;
        private readonly SemaphoreSlim audioRecordingLock = new SemaphoreSlim(1, 1);
        private readonly CancellationTokenSource audioRecordingCancellation = new CancellationTokenSource();
        private readonly ILogger logger;

        // Describes what the tablet should currently be recording. Distinguishes "nothing",
        // "whole interview" (audio audit flag) and a specific scoped group without overloading any
        // group id value, so a group whose id happens to be Guid.Empty cannot be confused with
        // whole-interview recording.
        private readonly struct RecordingTarget : IEquatable<RecordingTarget>
        {
            public static readonly RecordingTarget None = new RecordingTarget(false, null);
            public static readonly RecordingTarget WholeInterview = new RecordingTarget(true, null);
            public static RecordingTarget Group(Guid groupId) => new RecordingTarget(true, groupId);

            private RecordingTarget(bool isRecording, Guid? groupId)
            {
                this.IsRecording = isRecording;
                this.GroupId = groupId;
            }

            public bool IsRecording { get; }
            public Guid? GroupId { get; }

            public bool Equals(RecordingTarget other) =>
                this.IsRecording == other.IsRecording && this.GroupId == other.GroupId;

            public override bool Equals(object obj) => obj is RecordingTarget other && this.Equals(other);

            public override int GetHashCode() => HashCode.Combine(this.IsRecording, this.GroupId);
        }

        
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
                try
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

        private async Task StartAudioRecordingWithPermissionHandlingAsync(Guid interviewId)
        {
            await asyncDispatcher.ExecuteOnMainThreadAsync(async () =>
            {
                if (!this.isViewVisible)
                    return;
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

                var target = this.GetTargetRecording();

                // Already in the desired state.
                if (this.currentRecordingTarget.Equals(target))
                    return;

                // Stop the current recording when the target changed (group switch or leaving scope),
                // so each applicable group is captured as its own audio file, like the general audio audit.
                if (this.currentRecordingTarget.IsRecording)
                {
                    this.currentRecordingTarget = RecordingTarget.None;
                    audioAuditService.StopAudioRecording(interviewId);
                }

                // Start recording for the new target.
                if (target.IsRecording && !this.isAuditStarting)
                {
                    await this.StartAudioRecordingWithPermissionHandlingAsync(interviewId).ConfigureAwait(false);
                    // Only mark recording as active when the view is still visible; if start failed
                    // (e.g. missing permission) the helper navigates away and isViewVisible becomes false,
                    // so we must not store stale state that would prevent future retries.
                    if (this.isViewVisible)
                        this.currentRecordingTarget = target;
                }
            }
            finally
            {
                this.audioRecordingLock.Release();
            }
        }

        // Decides what should currently be recorded: the audio audit flag takes precedence (whole
        // interview), then the audio audit scope (applicable group), otherwise nothing.
        private RecordingTarget GetTargetRecording()
        {
            if (!this.isViewVisible)
                return RecordingTarget.None;

            if (IsAudioRecordingEnabled == true)
                return RecordingTarget.WholeInterview;

            var interview = interviewRepository.Get(this.InterviewId);
            if (interview == null)
                return RecordingTarget.None;

            var scope = interview.GetAudioAuditScope();
            if (scope == null || scope.Length == 0)
                return RecordingTarget.None;

            var currentGroup = this.NavigationState.CurrentGroup;
            if (currentGroup == null || !interview.ShouldRecordAudioForGroup(currentGroup))
                return RecordingTarget.None;

            return RecordingTarget.Group(currentGroup.Id);
        }

        // Stops any active recording under the recording lock so the state mutation is atomic with
        // EvaluateAudioRecordingAsync. Deliberately does not read interviewRepository or any other
        // per-interview state, so it is safe to run during teardown.
        private async Task StopAudioRecordingAsync(Guid interviewId)
        {
            await this.audioRecordingLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (this.currentRecordingTarget.IsRecording)
                {
                    this.currentRecordingTarget = RecordingTarget.None;
                    audioAuditService.StopAudioRecording(interviewId);
                }
            }
            finally
            {
                this.audioRecordingLock.Release();
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

                    // isViewVisible is already false. Stop any active recording through the same
                    // recording lock so the state mutation stays atomic with EvaluateAudioRecordingAsync.
                    // Run it off the UI thread so awaiting the lock cannot deadlock the UI thread against
                    // an in-flight start, and so teardown still completes after Dispose. The stop path
                    // touches neither interviewRepository nor any field disposed by this view model.
                    var recordingInterviewId = interviewId;
                    var recordingLogger = this.logger;
                    Task.Run(async () =>
                    {
                        try
                        {
                            await this.StopAudioRecordingAsync(recordingInterviewId).ConfigureAwait(false);
                        }
                        catch (Exception exc)
                        {
                            recordingLogger.Warn("Audio audit failed to stop on view disappearing.", exception: exc);
                        }
                    });
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
    this.audioRecordingCancellation.Dispose();
    base.Dispose();
}
    }
}
