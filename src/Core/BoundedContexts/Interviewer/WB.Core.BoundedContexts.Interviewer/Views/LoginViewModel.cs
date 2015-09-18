using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class LoginViewModel : BaseViewModel
    {
        readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IPrincipal principal;
        private readonly IPasswordHasher passwordHasher;
        private readonly IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage;

        public LoginViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            IPasswordHasher passwordHasher,
            IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.principal = principal;
            this.passwordHasher = passwordHasher;
            this.interviewersPlainStorage = interviewersPlainStorage;
        }

        public string UserName { get; set; }

        private string password;
        public string Password
        {
            get { return this.password; }
            set { this.password = value; RaisePropertyChanged(); }
        }

        private bool isUserValid;
        public bool IsUserValid
        {
            get { return this.isUserValid; }
            set { this.isUserValid = value; RaisePropertyChanged(); }
        }

        public IMvxCommand SignInCommand
        {
            get { return new MvxCommand(this.SignIn); }
        }

        public IMvxCommand NavigateToTroubleshootingPageCommand
        {
            get { return new MvxCommand(() => this.viewModelNavigationService.NavigateTo<TroubleshootingViewModel>()); }
        }

        public void Init()
        {
            InterviewerIdentity currentInterviewer =
                this.interviewersPlainStorage.Query(interviewers => interviewers.FirstOrDefault());

            if (currentInterviewer == null)
            {
                this.viewModelNavigationService.NavigateTo<FinishInstallationViewModel>();
                return;
            }

            this.IsUserValid = true;
            this.UserName = currentInterviewer.Name;
        }

        private void SignIn()
        {
            var userName = this.UserName;
            var hashedPassword = this.passwordHasher.Hash(this.Password);

            this.IsUserValid = this.principal.SignIn(userName, hashedPassword, true);

            if (this.IsUserValid)
                this.viewModelNavigationService.NavigateToDashboard();
        }

        public override void NavigateToPreviousViewModel()
        {

        }
    }
}