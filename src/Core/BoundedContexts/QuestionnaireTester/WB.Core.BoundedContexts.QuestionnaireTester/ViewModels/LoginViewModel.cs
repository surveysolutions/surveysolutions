using System.Net;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.ViewModels;

using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IRestService restService;
        private readonly IPrincipal principal;
        private readonly IUserInteraction uiDialogs;
        private readonly IErrorProcessor errorProcessor;


        public LoginViewModel(IRestService restService, IPrincipal principal, ILogger logger,
            IUserInteraction uiDialogs, IErrorProcessor errorProcessor) : base(logger)
        {
            this.restService = restService;
            this.principal = principal;
            this.uiDialogs = uiDialogs;
            this.errorProcessor = errorProcessor;
        }

        private bool isInProgress = false;
        public bool IsInProgress
        {
            get { return isInProgress; }
            set { isInProgress = value; RaisePropertyChanged(() => IsInProgress); }
        }

        private string loginName;
        public string LoginName
        {
            get { return loginName; }
            set { loginName = value; RaisePropertyChanged(() => LoginName); }
        }

        private string password;
        public string Password
        {
            get { return password; }
            set { password = value; RaisePropertyChanged(() => Password); }
        }

        private bool staySignedIn = true;
        public bool StaySignedIn
        {
            get { return staySignedIn; }
            set { staySignedIn = value; RaisePropertyChanged(() => StaySignedIn); }
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
                await
                    this.restService.GetAsync(url: "login",
                        credentials: new RestCredentials() {Login = LoginName, Password = Password});

                this.principal.SignIn(userName: LoginName, password: Password, rememberMe: StaySignedIn);

                this.ShowViewModel<DashboardViewModel>();
            }
            catch (RestException ex)
            {
                var error = this.errorProcessor.GetInternalErrorAndLogException(ex, TesterHttpAction.Login);

                switch (error.Code)
                {
                    case ErrorCode.UserWasNotAuthoredOnDesigner:
                    case ErrorCode.AccountIsLockedOnDesigner:
                    case ErrorCode.DesignerIsInMaintenanceMode:
                    case ErrorCode.DesignerIsUnavailable:
                    case ErrorCode.RequestTimeout:
                    case ErrorCode.RequestedUrlWasNotFound:
                    case ErrorCode.InternalServerError:
                        this.uiDialogs.Alert(error.Message);
                        break;

                    default: throw;
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
