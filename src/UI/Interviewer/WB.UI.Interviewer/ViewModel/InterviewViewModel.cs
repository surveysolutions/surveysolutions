using System;
using MvvmCross.Core.ViewModels;
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
            ILastCreatedInterviewStorage lastCreatedInterviewStorage)
            : base(questionnaireRepository, interviewRepository, sectionsViewModel,
                breadCrumbsViewModel, navigationState, answerNotifier, groupState, interviewState, coverState, principal, viewModelNavigationService,
                interviewViewModelFactory, commandService, jsonSerializer, vibrationViewModel, enumeratorSettings)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.lastCreatedInterviewStorage = lastCreatedInterviewStorage;
        }

        public override IMvxCommand ReloadCommand => new MvxCommand(() => this.viewModelNavigationService.NavigateToInterview(this.interviewId, this.navigationState.CurrentNavigationIdentity));

        public IMvxCommand NavigateToDashboardCommand => new MvxAsyncCommand(async () => {
            await this.viewModelNavigationService.NavigateToDashboard(this.interviewId);
            this.Dispose();
        });
        public IMvxCommand NavigateToDiagnosticsPageCommand => new MvxCommand(this.viewModelNavigationService.NavigateTo<DiagnosticsViewModel>);
        public IMvxCommand SignOutCommand => new MvxCommand(this.viewModelNavigationService.SignOutAndNavigateToLogin);

        public IMvxCommand NavigateToMapsCommand => new MvxCommand(this.NavigateToMaps);

        private void NavigateToMaps()
        {
            this.viewModelNavigationService.NavigateTo<MapsViewModel>();
        }

        public override void NavigateBack()
        {
            if (this.HasPrefilledQuestions && this.HasEdiablePrefilledQuestions)
            {
                this.viewModelNavigationService.NavigateToPrefilledQuestions(this.interviewId);
            }
            else
            {
                this.viewModelNavigationService.NavigateToDashboard(this.interviewId);
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
                    completeInterviewViewModel.Init(this.interviewId, this.navigationState);
                    return completeInterviewViewModel;
                case ScreenType.Cover:
                    var coverInterviewViewModel = this.interviewViewModelFactory.GetNew<CoverInterviewViewModel>();
                    coverInterviewViewModel.Init(this.interviewId, this.navigationState);
                    return coverInterviewViewModel;
                case ScreenType.Group:
                    var activeStageViewModel = this.interviewViewModelFactory.GetNew<EnumerationStageViewModel>();
                    activeStageViewModel.Init(this.interviewId, this.navigationState, eventArgs.TargetGroup, eventArgs.AnchoredElementIdentity);
                    return activeStageViewModel;
                default:
                    return null;
            }
        }

        public override void ViewAppeared()
        {
            if (!lastCreatedInterviewStorage.WasJustCreated(interviewId))
            {
                commandService.Execute(new ResumeInterviewCommand(Guid.Parse(interviewId), principal.CurrentUserIdentity.UserId, DateTime.Now, DateTime.UtcNow));
            }

            base.ViewAppeared();
        }

        public override void ViewDisappearing()
        {
            var interview = interviewRepository.Get(this.interviewId);
            if (!interview.IsCompleted)
            {
                commandService.Execute(new PauseInterviewCommand(Guid.Parse(interviewId), interview.CurrentResponsibleId, DateTime.Now, DateTime.UtcNow));
            }

            base.ViewDisappeared();
        }
    }
}