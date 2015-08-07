using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels.Groups;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class InterviewViewModel : EnumeratorInterviewViewModel
    {
        readonly IPrincipal principal;
        readonly IViewModelNavigationService viewModelNavigationService;

        public InterviewViewModel(IPrincipal principal,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IAnswerToStringService answerToStringService,
            SideBarSectionsViewModel sectionsViewModel,
            BreadCrumbsViewModel breadCrumbsViewModel,
            ActiveGroupViewModel groupViewModel,
            NavigationState navigationState,
            AnswerNotifier answerNotifier,
            IViewModelNavigationService viewModelNavigationService,
            GroupStateViewModel groupState)
            : base(questionnaireRepository, interviewRepository, answerToStringService, sectionsViewModel,
                breadCrumbsViewModel, groupViewModel, navigationState, answerNotifier, groupState)
        {
            this.principal = principal;
            this.viewModelNavigationService = viewModelNavigationService;
        }

        private IMvxCommand navigateToDashboardCommand;
        public IMvxCommand NavigateToDashboardCommand
        {
            get
            {
                return this.navigateToDashboardCommand ?? (this.navigateToDashboardCommand = new MvxCommand(() =>
                {
                    this.viewModelNavigationService.NavigateTo<DashboardViewModel>();
                }));
            }
        }

        private IMvxCommand navigateToHelpCommand;
        public IMvxCommand NavigateToHelpCommand
        {
            get
            {
                return this.navigateToHelpCommand ?? (this.navigateToHelpCommand = new MvxCommand(() =>
                {
                    this.viewModelNavigationService.NavigateTo<HelpViewModel>();
                }));
            }
        }

        private IMvxCommand signOutCommand;
        public IMvxCommand SignOutCommand
        {
            get { return this.signOutCommand ?? (this.signOutCommand = new MvxCommand(this.SignOut)); }
        }

        void SignOut()
        {
            this.principal.SignOut();
            this.viewModelNavigationService.NavigateTo<LoginViewModel>();
        }

        public override void NavigateToPreviousViewModel()
        {
            this.navigationState.NavigateBackAsync(this.NavigateBack).WaitAndUnwrapException();
        }

        void NavigateBack()
        {
            if (this.PrefilledQuestions.Any())
            {
                this.viewModelNavigationService.NavigateTo<PrefilledQuestionsViewModel>(new { interviewId = this.interviewId });
            }
            else
            {
                this.viewModelNavigationService.NavigateTo<DashboardViewModel>();
            }

        }
    }
}