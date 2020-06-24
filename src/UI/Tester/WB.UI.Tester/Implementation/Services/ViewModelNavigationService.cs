using System;
using System.Threading.Tasks;
using Android.Content;
using MvvmCross.Navigation;
using MvvmCross.Platforms.Android;
using MvvmCross.Plugin.Messenger;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.UI.Shared.Enumerator.CustomServices;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Tester.Activities;

namespace WB.UI.Tester.Implementation.Services
{
    public class ViewModelNavigationService : BaseViewModelNavigationService
    {
        private readonly IMvxAndroidCurrentTopActivity androidCurrentTopActivity;
        private readonly IMvxNavigationService navigationService;

        public ViewModelNavigationService(
            ICommandService commandService,
            IUserInteractionService userInteractionService,
            IUserInterfaceStateService userInterfaceStateService,
            IMvxAndroidCurrentTopActivity androidCurrentTopActivity,
            IPrincipal principal,
            IMvxNavigationService navigationService,
            ILogger log, IMvxMessenger messenger)
            : base(commandService, 
                userInteractionService, 
                userInterfaceStateService, 
                androidCurrentTopActivity, 
                navigationService, 
                principal, log)
        {
            this.androidCurrentTopActivity = androidCurrentTopActivity;
            this.navigationService = navigationService;
        }

        public override async Task<bool> NavigateToDashboardAsync(string interviewId = null)
        {
            return await this.navigationService.Navigate<DashboardViewModel>();
        }

        public override void NavigateToSplashScreen()
        {
            base.RestartApp(typeof(SplashActivity));
        }

        public override Task NavigateToPrefilledQuestionsAsync(string interviewId) => 
            this.navigationService.Navigate<InterviewViewModel, InterviewViewModelArgs>(new InterviewViewModelArgs
            {
                InterviewId = interviewId,
                NavigationIdentity = NavigationIdentity.CreateForCoverScreen()
            });

        public override Task NavigateToFinishInstallationAsync()
            => throw new NotImplementedException();

        public override Task NavigateToMapsAsync()
        => throw new NotImplementedException();

        public override Task NavigateToInterviewAsync(string interviewId, NavigationIdentity navigationIdentity)
            => this.navigationService.Navigate<InterviewViewModel, InterviewViewModelArgs>(new InterviewViewModelArgs
            {
                InterviewId = interviewId,
                NavigationIdentity = navigationIdentity
            });

        public override Task NavigateToCreateAndLoadInterview(int assignmentId)
        {
            throw new NotImplementedException();
        }

        public override Task NavigateToLoginAsync() => this.NavigateToAsync<LoginViewModel>();
        protected override void FinishActivity() => this.androidCurrentTopActivity.Activity.Finish();
        protected override void NavigateToSettingsImpl() =>
            this.androidCurrentTopActivity.Activity.StartActivity(new Intent(this.androidCurrentTopActivity.Activity, typeof(PrefsActivity)));
    }
}
