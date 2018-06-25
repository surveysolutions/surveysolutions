using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    public class DashboardMenuViewModel : MvxViewModel
    {
        private readonly IMvxNavigationService mvxNavigationService;

        public DashboardMenuViewModel(IMvxNavigationService mvxNavigationService)
        {
            this.mvxNavigationService = mvxNavigationService;
        }

        public IMvxCommand ShowCompletedInterviews => 
            new MvxAsyncCommand(async () => await mvxNavigationService.Navigate<DashboardCompletedInterviewsViewModel>());
    }
}
