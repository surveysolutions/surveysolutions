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
        public ViewModelNavigationService(
            ICommandService commandService,
            IUserInteractionService userInteractionService,
            IUserInterfaceStateService userInterfaceStateService,
            IPrincipal principal,
            ILogger log)
            : base(commandService, 
                userInteractionService, 
                userInterfaceStateService,
                principal, log)
        {
        }

        public override async Task<bool> NavigateToDashboardAsync(string interviewId = null)
        {
            return await NavigationService.Navigate<DashboardViewModel>();
        }

        public override void NavigateToSplashScreen()
        {
            base.RestartApp(typeof(SplashActivity));
        }

        public override Task NavigateToPrefilledQuestionsAsync(string interviewId) => 
            NavigationService.Navigate<InterviewViewModel, InterviewViewModelArgs>(new InterviewViewModelArgs
            {
                InterviewId = interviewId,
                NavigationIdentity = NavigationIdentity.CreateForCoverScreen()
            });

        public override Task NavigateToFinishInstallationAsync()
            => throw new NotImplementedException();

        public override Task NavigateToMapsAsync()
        => throw new NotImplementedException();

        public override Task NavigateToInterviewAsync(string interviewId, NavigationIdentity navigationIdentity)
            => NavigationService.Navigate<InterviewViewModel, InterviewViewModelArgs>(new InterviewViewModelArgs
            {
                InterviewId = interviewId,
                NavigationIdentity = navigationIdentity
            });

        public override Task NavigateToCreateAndLoadInterview(int assignmentId)
        {
            throw new NotImplementedException();
        }

        public override Task NavigateToLoginAsync() => this.NavigateToAsync<LoginViewModel>();
        protected override void FinishActivity() => TopActivity.Activity.Finish();
        protected override void NavigateToSettingsImpl() =>
            TopActivity.Activity.StartActivity(new Intent(TopActivity.Activity, typeof(PrefsActivity)));
    }
}
