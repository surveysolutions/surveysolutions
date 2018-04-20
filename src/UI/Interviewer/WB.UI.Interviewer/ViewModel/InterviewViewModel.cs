using System;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Implementation.AuditLog.Entities;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.UI.Interviewer.ViewModel
{
    public class InterviewViewModel : BaseInterviewViewModel
    {
        readonly IViewModelNavigationService viewModelNavigationService;
        private readonly ILastCreatedInterviewStorage lastCreatedInterviewStorage;
        private readonly IAuditLogService auditLogService;

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
            IJsonAllTypesSerializer jsonSerializer,
            VibrationViewModel vibrationViewModel,
            IEnumeratorSettings enumeratorSettings,
            ILastCreatedInterviewStorage lastCreatedInterviewStorage,
            IAuditLogService auditLogService)
            : base(questionnaireRepository, interviewRepository, sectionsViewModel,
                breadCrumbsViewModel, navigationState, answerNotifier, groupState, interviewState, coverState, principal, viewModelNavigationService,
                interviewViewModelFactory, commandService, vibrationViewModel, enumeratorSettings)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.lastCreatedInterviewStorage = lastCreatedInterviewStorage;
            this.auditLogService = auditLogService;
        }

        public override IMvxCommand ReloadCommand => new MvxAsyncCommand(async () => await this.viewModelNavigationService.NavigateToInterviewAsync(this.InterviewId, this.navigationState.CurrentNavigationIdentity));

        public IMvxCommand NavigateToDashboardCommand => new MvxAsyncCommand(async () => {
            auditLogService.Write(new CloseInterviewAuditLogEntity(this.InterviewId, this.InterviewKey));
            await this.viewModelNavigationService.NavigateToDashboardAsync(this.InterviewId);
            this.Dispose();
        });
        public IMvxCommand NavigateToDiagnosticsPageCommand => new MvxAsyncCommand(this.viewModelNavigationService.NavigateToAsync<DiagnosticsViewModel>);
        public IMvxCommand SignOutCommand => new MvxAsyncCommand(this.viewModelNavigationService.SignOutAndNavigateToLoginAsync);

        public IMvxCommand NavigateToMapsCommand => new MvxAsyncCommand(this.viewModelNavigationService.NavigateToAsync<MapsViewModel>);

        public override async Task NavigateBack()
        {
            if (this.HasPrefilledQuestions && this.HasEdiablePrefilledQuestions)
            {
                await this.viewModelNavigationService.NavigateToPrefilledQuestionsAsync(this.InterviewId);
            }
            else
            {
                auditLogService.Write(new CloseInterviewAuditLogEntity(this.InterviewId, this.InterviewKey));
                await this.viewModelNavigationService.NavigateToDashboardAsync(this.InterviewId);
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
                case ScreenType.Cover:
                    var coverInterviewViewModel = this.interviewViewModelFactory.GetNew<CoverInterviewViewModel>();
                    coverInterviewViewModel.Configure(this.InterviewId, this.navigationState);
                    return coverInterviewViewModel;
                case ScreenType.Group:
                    var activeStageViewModel = this.interviewViewModelFactory.GetNew<EnumerationStageViewModel>();
                    activeStageViewModel.Configure(this.InterviewId, this.navigationState, eventArgs.TargetGroup, eventArgs.AnchoredElementIdentity);
                    return activeStageViewModel;
                default:
                    return null;
            }
        }

        public override void ViewAppeared()
        {
            if (!lastCreatedInterviewStorage.WasJustCreated(InterviewId))
            {
                commandService.Execute(new ResumeInterviewCommand(Guid.Parse(InterviewId), Principal.CurrentUserIdentity.UserId, DateTime.Now, DateTime.UtcNow));
            }

            base.ViewAppeared();
        }

        public override void ViewDisappearing()
        {
            var interview = interviewRepository.Get(this.InterviewId);
            if (!interview.IsCompleted)
            {
                commandService.Execute(new PauseInterviewCommand(Guid.Parse(InterviewId), interview.CurrentResponsibleId, DateTime.Now, DateTime.UtcNow));
            }

            base.ViewDisappeared();
        }
    }
}
