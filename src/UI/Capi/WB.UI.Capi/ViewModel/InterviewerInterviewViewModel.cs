using System.Linq;
using Cirrious.MvvmCross.Binding.ExtensionMethods;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Capi.ViewModel;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.UI.Capi.Views;

namespace WB.UI.Capi.ViewModel
{
    public class InterviewerInterviewViewModel : EnumeratorInterviewViewModel
    {
        readonly IViewModelNavigationService viewModelNavigationService;

        public InterviewerInterviewViewModel(
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
            this.viewModelNavigationService = viewModelNavigationService;
        }


        private IMvxCommand navigateToDashboardCommand;
        public IMvxCommand NavigateToDashboardCommand
        {
            get { return this.navigateToDashboardCommand ?? (this.navigateToDashboardCommand = new MvxCommand(() => this.viewModelNavigationService.NavigateToDashboard())); }
        }

        private IMvxCommand signOutCommand;
        public IMvxCommand SignOutCommand
        {
            get { return this.signOutCommand ?? (this.signOutCommand = new MvxCommand(this.SignOut)); }
        }

        void SignOut()
        {
            CapiApplication.Membership.LogOff();
            this.viewModelNavigationService.NavigateTo<LoginActivityViewModel>();
        }

        public override void NavigateToPreviousViewModel()
        {
            this.navigationState.NavigateBackAsync(this.NavigateBack).WaitAndUnwrapException();
        }

        void NavigateBack()
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