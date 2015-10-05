using System;
using System.Linq;
using System.Threading.Tasks;

using Cirrious.MvvmCross.ViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
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
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IAnswerToStringService answerToStringService,
            SideBarSectionsViewModel sectionsViewModel,
            BreadCrumbsViewModel breadCrumbsViewModel,
            ActiveStageViewModel stageViewModel,
            NavigationState navigationState,
            AnswerNotifier answerNotifier,
            IViewModelNavigationService viewModelNavigationService,
            GroupStateViewModel groupState,
            InterviewStateViewModel interviewState)
            : base(questionnaireRepository, interviewRepository, answerToStringService, sectionsViewModel,
                breadCrumbsViewModel, stageViewModel, navigationState, answerNotifier, groupState, interviewState)
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

        private IMvxCommand navigateToHelpCommand;
        public IMvxCommand NavigateToHelpCommand
        {
            get
            {
                return this.navigateToHelpCommand ?? (this.navigateToHelpCommand = new MvxCommand(async () =>
                {
                    await this.viewModelNavigationService.NavigateToAsync<HelpViewModel>();
                }));
            }
        }

        private IMvxCommand signOutCommand;
        public IMvxCommand SignOutCommand
        {
            get { return this.signOutCommand ?? (this.signOutCommand = new MvxCommand(async () => await this.SignOut())); }
        }

        private async Task SignOut()
        {
            this.principal.SignOut();
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