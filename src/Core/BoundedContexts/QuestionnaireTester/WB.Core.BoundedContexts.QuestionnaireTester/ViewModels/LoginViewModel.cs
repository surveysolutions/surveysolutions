using System.Net;
using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IPrincipal principal;
        private readonly IDesignerApiService designerApiService;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IUserInteraction userInteraction;
        private readonly IFriendlyMessageService friendlyMessageService;

        public LoginViewModel(IPrincipal principal, IDesignerApiService designerApiService, IViewModelNavigationService viewModelNavigationService,
            IUserInteraction userInteraction, IFriendlyMessageService friendlyMessageService)
        {
            this.principal = principal;
            this.designerApiService = designerApiService;
            this.viewModelNavigationService = viewModelNavigationService;
            this.userInteraction = userInteraction;
            this.friendlyMessageService = friendlyMessageService;
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
                    this.viewModelNavigationService.NavigateTo<DashboardViewModel>();   
                }
            }
            catch (RestException ex)
            {
                string errorMessage = this.friendlyMessageService.GetFriendlyErrorMessageByRestException(ex);
                if (string.IsNullOrEmpty(errorMessage))
                {
                    switch (ex.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            errorMessage = UIResources.Login_Error_NotFound;
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(errorMessage))
                    this.userInteraction.Alert(errorMessage);
                else 
                    throw;
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
