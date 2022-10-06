using System;
using System.Threading.Tasks;
using Android.Content;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
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
            this.testerPrincipal = principal as ITesterPrincipal;
        }

        private readonly ITesterPrincipal testerPrincipal; 

        private bool IsRealUserAuthenticated => testerPrincipal.IsAuthenticated && !testerPrincipal.IsFakeIdentity;
        
        public override async Task<bool> NavigateToDashboardAsync(string interviewId = null)
        {
            return IsRealUserAuthenticated 
                ? await NavigationService.Navigate<DashboardViewModel>()
                : await NavigationService.Navigate<AnonymousQuestionnairesViewModel>();
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

        public override Task<bool> NavigateToInterviewAsync(string interviewId, NavigationIdentity navigationIdentity)
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
