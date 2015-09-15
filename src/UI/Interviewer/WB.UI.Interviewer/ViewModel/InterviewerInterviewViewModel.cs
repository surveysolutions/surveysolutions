using System.Linq;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.UI.Interviewer.ViewModel
{
    public class InterviewerInterviewViewModel : EnumeratorInterviewViewModel
    {
        readonly IStatefulInterviewRepository interviewRepository;
        readonly IViewModelNavigationService viewModelNavigationService;
        readonly IDataCollectionAuthentication authenticationService;

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
            IDataCollectionAuthentication authenticationService,
            GroupStateViewModel groupState)
            : base(questionnaireRepository, interviewRepository, answerToStringService, sectionsViewModel,
                breadCrumbsViewModel, groupViewModel, navigationState, answerNotifier, groupState)
        {
            this.interviewRepository = interviewRepository;
            this.viewModelNavigationService = viewModelNavigationService;
            this.authenticationService = authenticationService;
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
            authenticationService.LogOff();
            this.viewModelNavigationService.NavigateTo<LoginActivityViewModel>();
        }

        public override void NavigateToPreviousViewModel()
        {
            this.NavigateBack();
        }

        private void NavigateBack()
        {
            var interview = this.interviewRepository.Get(this.interviewId);
            if (this.PrefilledQuestions.Any() && interview.CreatedOnClient)
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