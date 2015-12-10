using System;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class DiagnosticsViewModel : BaseViewModel
    {
        private readonly IPrincipal principal;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IExternalAppLauncher externalAppLauncher;
        private readonly IInterviewerSettings interviewerSettings;

        public DiagnosticsViewModel(IPrincipal principal, 
            IViewModelNavigationService viewModelNavigationService,
            IInterviewerSettings interviewerSettings, 
            IExternalAppLauncher externalAppLauncher,
            SendTabletInformationViewModel sendTabletInformationViewModel)
        {
            this.principal = principal;
            this.viewModelNavigationService = viewModelNavigationService;
            this.interviewerSettings = interviewerSettings;
            this.externalAppLauncher = externalAppLauncher;
            this.TabletInformation = sendTabletInformationViewModel;
        }

        public void Init()
        {
            Version = this.interviewerSettings.GetApplicationVersionName();
            IsRestoreVisible = false;
        }

        public bool IsRestoreVisible
        {
            get { return this.isRestoreVisible; }
            set { this.isRestoreVisible = value; this.RaisePropertyChanged(); }
        }
        private bool isRestoreVisible;

        public string Version { get; set; }
        public SendTabletInformationViewModel TabletInformation { get; set; }

        public IMvxCommand ShareDeviceTechnicalInformationCommand => new MvxCommand(this.ShareDeviceTechnicalInformation);
        public IMvxCommand NavigateToDashboardCommand
        {
            get { return new MvxCommand(async () => await this.viewModelNavigationService.NavigateToAsync<LoginViewModel>()); }
        }

        public IMvxCommand SignOutCommand
        {
            get { return new MvxCommand(async () => await this.SignOut()); }
        }
        public IMvxCommand NavigateToLoginCommand
        {
            get { return new MvxCommand(async () => await this.viewModelNavigationService.NavigateToAsync<LoginViewModel>()); }
        }
        public bool IsAuthenticated => this.principal.IsAuthenticated;

        private void ShareDeviceTechnicalInformation()
        {
            this.externalAppLauncher.LaunchShareAction(InterviewerUIResources.Share_to_Title,
                this.interviewerSettings.GetDeviceTechnicalInformation());
        }


        private async Task SignOut()
        {
            this.principal.SignOut();
            await this.viewModelNavigationService.NavigateToAsync<LoginViewModel>();
        }
    }
}