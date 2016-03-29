using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.UI.Interviewer.ViewModel
{
    public class InterviewerInterviewViewModel : EnumeratorInterviewViewModel
    {
        readonly IStatefulInterviewRepository interviewRepository;
        readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IPrincipal principal;

        public InterviewerInterviewViewModel(
            IPlainQuestionnaireRepository questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IAnswerToStringService answerToStringService,
            SideBarSectionsViewModel sectionsViewModel,
            BreadCrumbsViewModel breadCrumbsViewModel,
            NavigationState navigationState,
            AnswerNotifier answerNotifier,
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            GroupStateViewModel groupState,
            InterviewStateViewModel interviewState,
            IInterviewViewModelFactory interviewViewModelFactory)
            : base(questionnaireRepository, interviewRepository, answerToStringService, sectionsViewModel,
                breadCrumbsViewModel, navigationState, answerNotifier, groupState, interviewState, principal, viewModelNavigationService,
                interviewViewModelFactory)
        {
            this.interviewRepository = interviewRepository;
            this.viewModelNavigationService = viewModelNavigationService;
            this.principal = principal;
        }


        private IMvxCommand navigateToDashboardCommand;
        public IMvxCommand NavigateToDashboardCommand
        {
            get { return this.navigateToDashboardCommand ?? (this.navigateToDashboardCommand = new MvxCommand(async () => await this.viewModelNavigationService.NavigateToDashboardAsync())); }
        }

        private IMvxCommand signOutCommand;
        public IMvxCommand SignOutCommand
        {
            get { return this.signOutCommand ?? (this.signOutCommand = new MvxCommand(async () => await this.SignOutAsync())); }
        }

        public IMvxCommand NavigateToDiagnosticsPageCommand
        {
            get { return new MvxCommand(async () => await this.viewModelNavigationService.NavigateToAsync<DiagnosticsViewModel>()); }
        }

        private async Task SignOutAsync()
        {
            await this.principal.SignOutAsync();
            await this.viewModelNavigationService.NavigateToLoginAsync();
        }

        public async Task NavigateToPreviousViewModelAsync(Action navigateToIfHistoryIsEmpty)
        {
            await this.navigationState.NavigateBackAsync(navigateToIfHistoryIsEmpty);
        }

        public async Task NavigateBack()
        {
            var interview = this.interviewRepository.Get(this.interviewId);
            if (this.PrefilledQuestions != null && this.PrefilledQuestions.Any() && interview.CreatedOnClient)
            {
                await this.viewModelNavigationService.NavigateToPrefilledQuestionsAsync(this.interviewId);
            }
            else
            {
                await this.viewModelNavigationService.NavigateToDashboardAsync();
            }
        }

        protected override async Task<MvxViewModel> UpdateCurrentScreenViewModel(ScreenChangedEventArgs eventArgs)
        {
            if (this.navigationState.CurrentScreenType == ScreenType.Complete)
            {
                var completeInterviewViewModel = interviewViewModelFactory.GetNew<InterviewerCompleteInterviewViewModel>();
                completeInterviewViewModel.Init(this.interviewId);
                return completeInterviewViewModel;
            }
            else
            {
                var activeStageViewModel = interviewViewModelFactory.GetNew<EnumerationStageViewModel>();
                await activeStageViewModel.InitAsync(interviewId, this.navigationState, eventArgs.TargetGroup, eventArgs.AnchoredElementIdentity);
                return activeStageViewModel;
            }
        }
    }
}