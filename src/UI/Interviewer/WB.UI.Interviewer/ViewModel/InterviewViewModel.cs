using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.UI.Interviewer.ViewModel
{
    public class InterviewViewModel : BaseInterviewViewModel
    {
        private readonly ILastCreatedInterviewStorage lastCreatedInterviewStorage;
        private readonly IAuditLogService auditLogService;
        private readonly IAudioAuditService audioAuditService;
        private readonly IUserInteractionService userInteractionService;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IJsonAllTypesSerializer serializer;

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
            ILastCreatedInterviewStorage lastCreatedInterviewStorage,
            IAuditLogService auditLogService,
            IAudioAuditService audioAuditService,
            IUserInteractionService userInteractionService,
            ILogger logger, 
            IPlainStorage<InterviewView> interviewViewRepository,
            IJsonAllTypesSerializer serializer)
            : base(questionnaireRepository, interviewRepository, sectionsViewModel,
                breadCrumbsViewModel, navigationState, answerNotifier, groupState, interviewState, coverState, principal, viewModelNavigationService,
                interviewViewModelFactory, commandService, vibrationViewModel, enumeratorSettings)
        {
            this.lastCreatedInterviewStorage = lastCreatedInterviewStorage;
            this.auditLogService = auditLogService;
            this.audioAuditService = audioAuditService;
            this.userInteractionService = userInteractionService;
            this.logger = logger;
            this.interviewViewRepository = interviewViewRepository;
            this.serializer = serializer;
        }

        static readonly TimeSpan PauseResumeThrottling = TimeSpan.FromSeconds(5);
        static readonly object ThrottlingLock = new Object();
        static PauseInterviewCommand pendingPause = null;
        private static Thread PauseThread;

        public override IMvxCommand ReloadCommand => new MvxAsyncCommand(async () => await this.viewModelNavigationService.NavigateToInterviewAsync(this.InterviewId, this.navigationState.CurrentNavigationIdentity));

        public IMvxCommand NavigateToMapsCommand => new MvxAsyncCommand(this.viewModelNavigationService.NavigateToAsync<MapsViewModel>);

        public override async Task NavigateBack()
        {
            if (this.HasPrefilledQuestions && this.HasEdiablePrefilledQuestions)
            {
                await this.viewModelNavigationService.NavigateToPrefilledQuestionsAsync(this.InterviewId);
            }
            else
            {
                await this.viewModelNavigationService.NavigateToDashboardAsync(this.InterviewId);
                this.Dispose();
            }
        }

        protected override NavigationIdentity GetDefaultScreenToNavigate(IQuestionnaire questionnaire)
        {
            if (HasNotEmptyNoteFromSupervior || HasCommentsFromSupervior || HasPrefilledQuestions)
                return NavigationIdentity.CreateForCoverScreen();

            return base.GetDefaultScreenToNavigate(questionnaire);
        }

        protected override MvxViewModel UpdateCurrentScreenViewModel(ScreenChangedEventArgs eventArgs)
        {
            switch (this.navigationState.CurrentScreenType)
            {
                case ScreenType.Complete:
                    var completeInterviewViewModel = this.interviewViewModelFactory.GetNew<InterviewerCompleteInterviewViewModel>();
                    completeInterviewViewModel.Configure(this.InterviewId, this.navigationState);
                    return completeInterviewViewModel;
                default:
                    return base.UpdateCurrentScreenViewModel(eventArgs);
            }
        }

        public override void ViewAppeared()
        {
            if (!this.Principal.IsAuthenticated)
            {
                this.viewModelNavigationService.NavigateToLoginAsync().WaitAndUnwrapException();
                return;
            }

            Task.Run(async () =>
            {
                var interviewId = Guid.Parse(InterviewId);
                var interview = interviewRepository.Get(this.InterviewId);
                if (interview == null) return;

                if (!lastCreatedInterviewStorage.WasJustCreated(InterviewId))
                {
                    lock (ThrottlingLock)
                    {
                        PauseThread?.Abort();
                        if (pendingPause == null)
                        {
                            commandService.Execute(new ResumeInterviewCommand(interviewId, Principal.CurrentUserIdentity.UserId));
                        }
                        else
                        {
                            var delay = DateTimeOffset.Now - pendingPause.OriginDate;
                            if (delay > PauseResumeThrottling && pendingPause.InterviewId == interviewId)
                            {
                                commandService.Execute(pendingPause);
                                commandService.Execute(new ResumeInterviewCommand(interviewId, Principal.CurrentUserIdentity.UserId));
                            }
                        }

                        pendingPause = null;
                    }
                }

                if (IsAudioRecordingEnabled == true && !isAuditStarting)
                {
                    isAuditStarting = true;
                    try
                    {
                        await audioAuditService.StartAudioRecordingAsync(interviewId).ConfigureAwait(false);
                    }
                    catch (Exception exc)
                    {
                        logger.Warn("Audio audit failed to start.", exception: exc);
                        await this.viewModelNavigationService.NavigateToDashboardAsync(this.InterviewId)
                                .ConfigureAwait(false);
                        this.userInteractionService.ShowToast(exc.Message);
                    }
                    finally
                    {
                        isAuditStarting = false;
                    }
                }

                auditLogService.Write(new OpenInterviewAuditLogEntity(interviewId, interviewKey?.ToString(), assignmentId));
                base.ViewAppeared();
            });
        }

        private bool isAuditStarting = false;
        
        private readonly ILogger logger;

        public override void ViewDisappearing()
        {
            if (InterviewId != null && Principal.IsAuthenticated)
            {
                var interview = interviewRepository.Get(this.InterviewId);
                if (interview != null)
                {
                    var interviewId = interview.Id;

                    if (!interview.IsCompleted)
                    {
                        pendingPause = new PauseInterviewCommand(interviewId, Principal.CurrentUserIdentity.UserId);
                        var cmdid = pendingPause.CommandIdentifier; // to make sure it's a same command

                        // discard - c# feature to ignore warning on non awaited Task
                        PauseThread = new Thread(() => { PauseInterview(interviewId, cmdid); });
                        PauseThread.Start();
                    }

                    auditLogService.Write(new CloseInterviewAuditLogEntity(interviewId, interviewKey?.ToString()));

                    if (IsAudioRecordingEnabled == true)
                        audioAuditService.StopAudioRecording(interviewId);
                }
            }

            var interviewView = this.interviewViewRepository.GetById(InterviewId);
            if (interviewView != null)
            {
                if (this.navigationState.CurrentGroup != null)
                {
                    interviewView.LastVisitedSectionId = this.serializer.Serialize(this.navigationState.CurrentGroup);
                }
                interviewView.LastVisitedScreenType = this.navigationState.CurrentScreenType;
                this.interviewViewRepository.Store(interviewView);
            }
            
            base.ViewDisappearing();
        }

        private void PauseInterview(Guid interviewId, Guid cmdid)
        {
            Thread.Sleep(PauseResumeThrottling);

            lock (ThrottlingLock)
            {
                if (pendingPause != null)
                {
                    var delay = DateTimeOffset.Now - pendingPause.OriginDate;
                    var sameInterview = pendingPause.InterviewId == interviewId;
                    var samePendingCommand = pendingPause.CommandIdentifier == cmdid;

                    if (delay > PauseResumeThrottling
                        && sameInterview
                        && samePendingCommand
                        && interviewRepository.Get(this.InterviewId) != null) //could be synced and deleted
                    {
                        try
                        {
                            commandService.Execute(pendingPause);
                        }
                        catch (Exception e)
                        {
                            logger.Info($"Was not able to save pause event for {pendingPause.InterviewId}", e);
                        }
                    }
                }

                pendingPause = null;
            }
        }
    }
}
