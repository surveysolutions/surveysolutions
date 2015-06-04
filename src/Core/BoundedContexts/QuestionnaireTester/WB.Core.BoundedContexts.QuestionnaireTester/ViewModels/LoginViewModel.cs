using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IPrincipal principal;
        private readonly DesignerApiService designerApiService;

        public LoginViewModel(IPrincipal principal, DesignerApiService designerApiService)
        {
            this.principal = principal;
            this.designerApiService = designerApiService;
        }

        private bool isInProgress = false;
        public bool IsInProgress
        {
            get { return isInProgress; }
            set { isInProgress = value; RaisePropertyChanged(); }
        }

        private string loginName;
        public string LoginName
        {
            get { return loginName; }
            set { loginName = value; RaisePropertyChanged(); }
        }

        private string password;
        public string Password
        {
            get { return password; }
            set { password = value; RaisePropertyChanged(); }
        }

        private bool staySignedIn = true;
        public bool StaySignedIn
        {
            get { return staySignedIn; }
            set { staySignedIn = value; RaisePropertyChanged(); }
        }

        public IMvxCommand LoginCommand
        {
            get { return new MvxCommand(this.Login, () => !IsInProgress); }
        }

        private async void Login()
        {
            IsInProgress = true;

            try
            {
                if (await this.designerApiService.Authorize(login: LoginName, password: Password))
                {
                    this.principal.SignIn(userName: LoginName, password: Password, rememberMe: StaySignedIn);
                    this.ShowViewModel<DashboardViewModel>();   
                }
            }
            finally
            {
                IsInProgress = false;    
            }
        }

        public override void NavigateToPreviousViewModel()
        {
            
        }
    }
}
