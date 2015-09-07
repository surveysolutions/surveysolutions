using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class FinishIntallationViewModel : BaseViewModel
    {
        private readonly IInterviewerSettings interviewerSettings;
        readonly IViewModelNavigationService viewModelNavigationService;
        private readonly ISynchronizationService synchronizationService;
        private readonly IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage;
        private readonly IPrincipal principal;
        private readonly IPasswordHasher passwordHasher;

        public FinishIntallationViewModel(
            IInterviewerSettings interviewerSettings, 
            IViewModelNavigationService viewModelNavigationService,
            ISynchronizationService synchronizationService,
            IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage,
            IPrincipal principal,
            IPasswordHasher passwordHasher)
        {
            this.interviewerSettings = interviewerSettings;
            this.viewModelNavigationService = viewModelNavigationService;
            this.synchronizationService = synchronizationService;
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.principal = principal;
            this.passwordHasher = passwordHasher;
        }

        public string ServerAddress { get; private set; }
        public string Login { get; private set; }
        public string Password { get; private set; }

        private IMvxCommand loginCommand;
        public IMvxCommand LoginCommand
        {
            get { return this.loginCommand ?? (this.loginCommand = new MvxCommand(async () => await this.LoginAsync())); }
        }

        private async Task LoginAsync()
        {
            this.interviewerSettings.SetSyncAddressPoint(this.ServerAddress);

            var userName = this.Login;
            var hashedPassword = this.passwordHasher.Hash(this.Password);
            try
            {
                var interviewer = await this.synchronizationService.GetCurrentInterviewerAsync(login: userName, password: hashedPassword);
                await this.interviewersPlainStorage.StoreAsync(new InterviewerIdentity()
                {
                    Id = interviewer.Id.FormatGuid(),
                    UserId = interviewer.Id,
                    SupervisorId = interviewer.SupervisorId,
                    Name = userName,
                    Password = hashedPassword
                });
                this.principal.SignIn(userName, hashedPassword, true);

                if (!await this.synchronizationService.HasCurrentInterviewerDeviceAsync())
                {
                    await this.synchronizationService.LinkCurrentInterviewerToDeviceAsync();
                    this.viewModelNavigationService.NavigateTo<DashboardViewModel>();
                    return;
                }

                if (!await this.synchronizationService.IsDeviceLinkedToCurrentInterviewerAsync())
                {
                    this.viewModelNavigationService.NavigateTo<RelinkDeviceViewModel>(new { redirectedFromFinishInstallation = true });
                }
                else
                {
                    this.viewModelNavigationService.NavigateTo<DashboardViewModel>();
                }
            }
            finally
            {
                
            }
            
        }

        public void Init()
        {
#if DEBUG
            this.ServerAddress = "http://192.168.1.146/headquarters";
            this.Login = "int";
            this.Password = "1";
#endif
        }

        public override void NavigateToPreviousViewModel()
        {
            
        }
    }
}