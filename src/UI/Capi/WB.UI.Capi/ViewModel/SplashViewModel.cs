using System.Threading.Tasks;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Capi.ViewModel
{
    public class SplashViewModel : BaseViewModel
    {
        private readonly IInterviewerSettings interviewerSettings;
        private readonly IViewModelNavigationService viewModelNavigationService;

        public SplashViewModel(IInterviewerSettings interviewerSettings, IViewModelNavigationService viewModelNavigationService)
        {
            this.interviewerSettings = interviewerSettings;
            this.viewModelNavigationService = viewModelNavigationService;
        }

        public override async void Start()
        {
            await Task.Delay(3000);

            if (CapiApplication.Membership.IsLoggedIn)
            {
                this.viewModelNavigationService.NavigateToDashboard();
            }
            else if (this.interviewerSettings.GetClientRegistrationId() == null)
            {
                this.viewModelNavigationService.NavigateTo<FinishIntallationViewModel>();
            }
            else
            {
                this.viewModelNavigationService.NavigateTo<LoginActivityViewModel>();
            }
        }

        public override void NavigateToPreviousViewModel()
        {

        }
    }
}