using Android.Content;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.Droid.Platform;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.UI.Interviewer.Activities;
using WB.UI.Interviewer.ViewModel;

namespace WB.UI.Interviewer.Implementations.Services
{
    internal class ViewModelNavigationService : BaseViewModelNavigationService, IViewModelNavigationService
    {
        private readonly IMvxAndroidCurrentTopActivity androidCurrentTopActivity;
        private readonly IJsonAllTypesSerializer jsonSerializer;
        private readonly IMvxNavigationService navigationService;

        public ViewModelNavigationService(
            ICommandService commandService,
            IUserInteractionService userInteractionService,
            IUserInterfaceStateService userInterfaceStateService,
            IMvxAndroidCurrentTopActivity androidCurrentTopActivity,
            IPrincipal principal,
            IJsonAllTypesSerializer jsonSerializer,
            IMvxNavigationService navigationService)
            : base(commandService, userInteractionService, userInterfaceStateService, principal)
        {
            this.androidCurrentTopActivity = androidCurrentTopActivity;
            this.jsonSerializer = jsonSerializer;
            this.navigationService = navigationService;
        }

        public void NavigateTo<TViewModel>() where TViewModel : IMvxViewModel => this.NavigateTo<TViewModel>(null);

        public void NavigateToDashboard()
        {
            this.navigationService.Navigate<DashboardViewModel, DashboardArgs>(new DashboardArgs());
        }

        public void NavigateToPrefilledQuestions(string interviewId) => this.NavigateTo<PrefilledQuestionsViewModel>(new { interviewId = interviewId });

        public void NavigateToInterview(string interviewId, NavigationIdentity navigationIdentity)
            => this.NavigateTo<InterviewViewModel>(new
            {
                interviewId = interviewId,
                jsonNavigationIdentity = navigationIdentity != null ? this.jsonSerializer.Serialize(navigationIdentity) : null
            });

        public override void NavigateToLogin() => this.NavigateTo<LoginViewModel>();
        protected override void FinishActivity() => this.androidCurrentTopActivity.Activity.Finish();
        protected override void NavigateToSettingsImpl() =>
            this.androidCurrentTopActivity.Activity.StartActivity(new Intent(this.androidCurrentTopActivity.Activity, typeof(PrefsActivity)));
    }
}