using System;
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
            ILogger logger)
            : base(questionnaireRepository, interviewRepository, sectionsViewModel,
                breadCrumbsViewModel, navigationState, answerNotifier, groupState, interviewState, coverState, principal, viewModelNavigationService,
                interviewViewModelFactory, commandService, vibrationViewModel, enumeratorSettings)
        {
            this.lastCreatedInterviewStorage = lastCreatedInterviewStorage;
            this.auditLogService = auditLogService;
            this.audioAuditService = audioAuditService;
            this.userInteractionService = userInteractionService;
            this.logger = logger;
        }

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
                
                commandService.Execute(new ResumeInterviewCommand(interviewId,
                    Principal.CurrentUserIdentity.UserId));

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

                    var pause = new PauseInterviewCommand(interviewId, Principal.CurrentUserIdentity.UserId);
                    commandService.Execute(pause);

                    auditLogService.Write(new CloseInterviewAuditLogEntity(interviewId, interviewKey?.ToString()));

                    if (IsAudioRecordingEnabled == true)
                        audioAuditService.StopAudioRecording(interviewId);
                }
            }

            base.ViewDisappearing();
        }
    }
}
