using System.Linq;
using Android.Content;
using Cirrious.CrossCore.Droid.Platform;
using Cirrious.MvvmCross.Binding.ExtensionMethods;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.ViewModels.Groups;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.Capi.ViewModel
{
    public class InterviewerInterviewViewModel : EnumeratorInterviewViewModel
    {
        readonly IViewModelNavigationService viewModelNavigationService;
        readonly IMvxAndroidCurrentTopActivity mvxAndroidCurrentTopActivity;

        public InterviewerInterviewViewModel(
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IAnswerToUIStringService answerToUIStringService,
            SideBarSectionsViewModel sectionsViewModel,
            BreadCrumbsViewModel breadCrumbsViewModel,
            ActiveGroupViewModel groupViewModel,
            NavigationState navigationState,
            AnswerNotifier answerNotifier,
            IViewModelNavigationService viewModelNavigationService,
            GroupStateViewModel groupState,
            IMvxAndroidCurrentTopActivity mvxAndroidCurrentTopActivity)
            : base(questionnaireRepository, interviewRepository, answerToUIStringService, sectionsViewModel,
                breadCrumbsViewModel, groupViewModel, navigationState, answerNotifier, groupState)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.mvxAndroidCurrentTopActivity = mvxAndroidCurrentTopActivity;
        }


        private IMvxCommand navigateToDashboardCommand;
        public IMvxCommand NavigateToDashboardCommand
        {
            get { return this.navigateToDashboardCommand ?? (this.navigateToDashboardCommand = new MvxCommand(this.CreateAndStartActivity<DashboardActivity>)); }
        }

        private IMvxCommand signOutCommand;
        public IMvxCommand SignOutCommand
        {
            get { return this.signOutCommand ?? (this.signOutCommand = new MvxCommand(this.SignOut)); }
        }

        void SignOut()
        {
            CapiApplication.Membership.LogOff();
            this.CreateAndStartActivity<LoginActivity>();
        }

        public override void NavigateToPreviousViewModel()
        {
            this.navigationState.NavigateBackAsync(this.NavigateBack).WaitAndUnwrapException();
        }

        void NavigateBack()
        {
            if (this.PrefilledQuestions.Any())
            {
                this.viewModelNavigationService.NavigateTo<InterviewerPrefilledQuestionsViewModel>(new {interviewId = this.interviewId});
            }
            else
            {
                this.CreateAndStartActivity<DashboardActivity>();
            }
            
        }

        void CreateAndStartActivity<TActivity>()
        {
            var intent = new Intent(mvxAndroidCurrentTopActivity.Activity, typeof(TActivity));
            intent.AddFlags(ActivityFlags.NoHistory);
            mvxAndroidCurrentTopActivity.Activity.StartActivity(intent);
        }
    }
}