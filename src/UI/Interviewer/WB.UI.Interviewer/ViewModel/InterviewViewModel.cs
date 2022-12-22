using System;
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
        }

        public override IMvxCommand ReloadCommand => new MvxAsyncCommand(async () =>
            await this.ViewModelNavigationService.NavigateToInterviewAsync(this.InterviewId,
                this.NavigationState.CurrentNavigationIdentity));

        public IMvxCommand NavigateToMapsCommand =>
            new MvxAsyncCommand(this.ViewModelNavigationService.NavigateToAsync<MapsViewModel>);

        public override async Task NavigateBack()
        {
            await this.ViewModelNavigationService.NavigateToDashboardAsync(this.InterviewId);
            this.Dispose();
        }

        protected override NavigationIdentity GetDefaultScreenToNavigate(IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            if (HasNotEmptyNoteFromSupervior || HasCommentsFromSupervior || HasPrefilledQuestions)
                return NavigationIdentity.CreateForCoverScreen();

            return base.GetDefaultScreenToNavigate(interview, questionnaire);
        }

        protected override MvxViewModel UpdateCurrentScreenViewModel(ScreenChangedEventArgs eventArgs)
        {
            switch (this.NavigationState.CurrentScreenType)
            {
                case ScreenType.Complete:
                    var completeInterviewViewModel =
                        this.interviewViewModelFactory.GetNew<InterviewerCompleteInterviewViewModel>();
                    completeInterviewViewModel.Configure(this.InterviewId, this.NavigationState);
                    return completeInterviewViewModel;
                default:
                    return base.UpdateCurrentScreenViewModel(eventArgs);
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

                if (IsAudioRecordingEnabled == true && !isAuditStarting)
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

                auditLogService.Write(new OpenInterviewAuditLogEntity(interviewId, interviewKey?.ToString(),
                    assignmentId));
                base.ViewAppeared();
            });
        }
        
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
    }
}
