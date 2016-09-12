using System.Net;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Properties;
using WB.Core.BoundedContexts.Tester.Views;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.UI.Tester.Infrastructure.Internals.Security;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IDesignerApiService designerApiService;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IUserInteractionService userInteractionService;
        private readonly IFriendlyErrorMessageService friendlyErrorMessageService;

        private readonly IAsyncPlainStorage<TesterUserIdentity> userStorage;
        private readonly IAsyncPlainStorage<QuestionnaireListItem> questionnairesStorage;
        private readonly IAsyncPlainStorage<DashboardLastUpdate> dashboardLastUpdateStorage;

        public LoginViewModel(
            IPrincipal principal, 
            IDesignerApiService designerApiService, 
            IViewModelNavigationService viewModelNavigationService,
            IUserInteractionService userInteractionService, 
            IFriendlyErrorMessageService friendlyErrorMessageService,
            IAsyncPlainStorage<QuestionnaireListItem> questionnairesStorage, 
            IAsyncPlainStorage<DashboardLastUpdate> dashboardLastUpdateStorage,
            IAsyncPlainStorage<TesterUserIdentity> userStorage)
            : base(principal, viewModelNavigationService)
        {
            this.designerApiService = designerApiService;
            this.viewModelNavigationService = viewModelNavigationService;
            this.userInteractionService = userInteractionService;
            this.friendlyErrorMessageService = friendlyErrorMessageService;

            this.questionnairesStorage = questionnairesStorage;
            this.dashboardLastUpdateStorage = dashboardLastUpdateStorage;
            this.userStorage = userStorage;
        }

        public override bool IsAuthenticationRequired => false;

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
            get { return new MvxCommand(async () => await this.LoginAsync(), () => !IsInProgress); }
        }

        private async Task LoginAsync()
        {
            IsInProgress = true;

            string errorMessage = null;

            try
            {
                if (await this.designerApiService.Authorize(login: LoginName, password: Password))
                {
                    this.userStorage.RemoveAll();
                    this.dashboardLastUpdateStorage.RemoveAll();
                    this.questionnairesStorage.RemoveAll();

                    this.principal.SignIn(userName: this.LoginName, password: this.Password, staySignedIn: this.StaySignedIn);
                    this.viewModelNavigationService.NavigateTo<DashboardViewModel>();   
                }
            }
            catch (RestException ex)
            {
                switch (ex.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        errorMessage = TesterUIResources.Login_Error_NotFound;
                        break;
                    default:
                        errorMessage = this.friendlyErrorMessageService.GetFriendlyErrorMessageByRestException(ex);
                        break;
                }

                if (string.IsNullOrEmpty(errorMessage))
                    throw;
            }
            finally
            {
                IsInProgress = false;    
            }

            if (!string.IsNullOrEmpty(errorMessage))
                await this.userInteractionService.AlertAsync(errorMessage);
        }
    }
}
