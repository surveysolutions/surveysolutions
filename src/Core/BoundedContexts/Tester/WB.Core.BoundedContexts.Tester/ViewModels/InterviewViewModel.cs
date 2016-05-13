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

        private IMvxCommand navigateToDashboardCommand;
        public IMvxCommand NavigateToDashboardCommand
        {
            get
            {
                return this.navigateToDashboardCommand ?? (this.navigateToDashboardCommand = new MvxCommand(async () =>
                {
                    await this.viewModelNavigationService.NavigateToAsync<DashboardViewModel>();
                }));
            }
        }

        private IMvxCommand signOutCommand;
        public IMvxCommand SignOutCommand
        {
            get { return this.signOutCommand ?? (this.signOutCommand = new MvxCommand(async () => await this.SignOutAsync())); }
        }

        private async Task SignOutAsync()
        {
            await this.principal.SignOutAsync();
            await this.viewModelNavigationService.NavigateToAsync<LoginViewModel>();
        }

        public async Task NavigateToPreviousViewModelAsync(Action navigateToIfHistoryIsEmpty)
        {
            await this.navigationState.NavigateBackAsync(navigateToIfHistoryIsEmpty);
        }

        public async Task NavigateBack()
        {
            if (this.PrefilledQuestions.Any())
            {
                await this.viewModelNavigationService.NavigateToPrefilledQuestionsAsync(this.interviewId);
            }
            else
            {
                await this.viewModelNavigationService.NavigateToDashboardAsync();
            }

        }
    }
}