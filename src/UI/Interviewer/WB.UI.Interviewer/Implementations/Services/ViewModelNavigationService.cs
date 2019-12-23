using System;
using System.Threading.Tasks;
using Android.Content;
using MvvmCross.Navigation;
using MvvmCross.Platforms.Android;
using MvvmCross.Plugin.Messenger;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Interviewer.Activities;
using WB.UI.Interviewer.ViewModel;
using WB.UI.Shared.Enumerator.Services;

namespace WB.UI.Interviewer.Implementations.Services
{
    internal class ViewModelNavigationService : BaseViewModelNavigationService
    {
        private readonly IMvxAndroidCurrentTopActivity androidCurrentTopActivity;
        private readonly IMvxNavigationService navigationService;
        private readonly ILogger log;

        public ViewModelNavigationService(
            ICommandService commandService,
            IUserInteractionService userInteractionService,
            IUserInterfaceStateService userInterfaceStateService,
            IMvxAndroidCurrentTopActivity androidCurrentTopActivity,
            IPrincipal principal,
            IMvxNavigationService navigationService,
            ILogger log,
            IMvxMessenger messenger)
            : base(commandService, 
                userInteractionService, 
                userInterfaceStateService, 
                androidCurrentTopActivity, 
                navigationService, principal, log, 
                messenger)
        {
            this.androidCurrentTopActivity = androidCurrentTopActivity;
            this.navigationService = navigationService;
            this.log = log;
        }

        public override async Task<bool> NavigateToDashboardAsync(string interviewId = null)
        {
            this.log.Trace($"Navigating to dashboard interviewId: {interviewId ?? "'null'"}");
            if (interviewId == null)
            {
               return await this.navigationService.Navigate<DashboardViewModel>().ConfigureAwait(false);
            }

            return await this.navigationService.Navigate<DashboardViewModel, DashboardViewModelArgs>(new DashboardViewModelArgs
            {
                InterviewId = Guid.Parse(interviewId)
            }).ConfigureAwait(false);
        }

        public override Task NavigateToPrefilledQuestionsAsync(string interviewId)
        {
            this.log.Trace($"Navigating to PrefilledQuestionsViewModel interviewId: {interviewId}");
            return this.navigationService.Navigate<PrefilledQuestionsViewModel, InterviewViewModelArgs>(
                new InterviewViewModelArgs
                {
                    InterviewId = interviewId
                });
        }

        public override void NavigateToSplashScreen()
        {
            base.RestartApp(typeof(SplashActivity));
        }

        public override Task NavigateToFinishInstallationAsync()
        {
            this.log.Trace("Navigating to FinishInstallationViewModel");
            return this.NavigateToAsync<FinishInstallationViewModel>();
        }

        public override Task NavigateToMapsAsync()
        {
            this.log.Trace("Navigating to MapsViewModel");
            return this.NavigateToAsync<MapsViewModel>();
        }

        public override Task NavigateToInterviewAsync(string interviewId, NavigationIdentity navigationIdentity)
        {
            this.log.Trace($"Navigating to interview {interviewId}:{navigationIdentity}");
            return this.navigationService.Navigate<InterviewViewModel, InterviewViewModelArgs>(
                new InterviewViewModelArgs
                {
                    InterviewId = interviewId,
                    NavigationIdentity = navigationIdentity
                });
        }

        public override Task NavigateToLoginAsync()
        {
            this.log.Trace("Navigating to login");
            return this.NavigateToAsync<LoginViewModel>();
        }

        protected override void FinishActivity() => this.androidCurrentTopActivity.Activity.Finish();
        protected override void NavigateToSettingsImpl() =>
            this.androidCurrentTopActivity.Activity.StartActivity(new Intent(this.androidCurrentTopActivity.Activity, typeof(PrefsActivity)));
    }
}
