using System;
using System.Linq;
using System.Threading.Tasks;

using MvvmCross.Core.ViewModels;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class InterviewViewModel : EnumeratorInterviewViewModel
    {
        private readonly IPrincipal principal;
        private readonly IViewModelNavigationService viewModelNavigationService;

        public InterviewViewModel(IPrincipal principal,
            IPlainQuestionnaireRepository questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IAnswerToStringService answerToStringService,
            SideBarSectionsViewModel sectionsViewModel,
            BreadCrumbsViewModel breadCrumbsViewModel,
            NavigationState navigationState,
            AnswerNotifier answerNotifier,
            IViewModelNavigationService viewModelNavigationService,
            GroupStateViewModel groupState,
            InterviewStateViewModel interviewState,
            IInterviewViewModelFactory interviewViewModelFactory)
            : base(questionnaireRepository, interviewRepository, answerToStringService, sectionsViewModel,
                breadCrumbsViewModel, navigationState, answerNotifier, groupState, interviewState, principal, viewModelNavigationService,
                interviewViewModelFactory)
        {
            this.principal = principal;
            this.viewModelNavigationService = viewModelNavigationService;
        }

        public IMvxCommand NavigateToSettingsCommand
            => new MvxCommand(this.viewModelNavigationService.NavigateToSettings);

        public IMvxCommand NavigateToDashboardCommand => new MvxCommand(this.viewModelNavigationService.NavigateToDashboard);
        
        public IMvxCommand SignOutCommand => new MvxCommand(this.viewModelNavigationService.SignOutAndNavigateToLogin);

        public void NavigateToPreviousViewModel(Action navigateToIfHistoryIsEmpty)
            => this.navigationState.NavigateBack(navigateToIfHistoryIsEmpty);

        public void NavigateBack()
        {
            if (this.PrefilledQuestions.Any())
            {
                this.viewModelNavigationService.NavigateToPrefilledQuestions(this.interviewId);
            }
            else
            {
                this.viewModelNavigationService.NavigateToDashboard();
            }

        }
    }
}