using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class RelinkDeviceViewModel : BaseViewModel
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly ISynchronizationService synchronizationService;
        private readonly IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage;

        public RelinkDeviceViewModel(
            IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            ISynchronizationService synchronizationService,
            IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage)
            : base(principal, viewModelNavigationService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.synchronizationService = synchronizationService;
            this.interviewersPlainStorage = interviewersPlainStorage;
        }

        public override bool IsAuthenticationRequired => false;

        private string errorMessage;
        public string ErrorMessage
        {
            get { return this.errorMessage; }
            set { this.errorMessage = value; RaisePropertyChanged(); }
        }

        private bool isInProgress;
        public bool IsInProgress
        {
            get { return this.isInProgress; }
            set { this.isInProgress = value; RaisePropertyChanged(); }
        }

        public IMvxCommand CancelCommand => new MvxCommand(this.NavigateToPreviousViewModel, () => !this.IsInProgress);

        public IMvxCommand NavigateToDiagnosticsPageCommand
            => new MvxCommand(() => this.viewModelNavigationService.NavigateTo<DiagnosticsViewModel>(),
                () => !this.IsInProgress);

        private IMvxCommand relinkCommand;
        public IMvxCommand RelinkCommand
        {
            get
            {
                return relinkCommand ?? (relinkCommand =
                    new MvxCommand(async () => await this.RelinkCurrentInterviewerToDeviceAsync(),
                        () => !this.IsInProgress));
            }
        }

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private InterviewerIdentity userIdentityToRelink;

        public void Init(InterviewerIdentity userIdentity)
        {
            this.userIdentityToRelink = userIdentity;
        }

        private async Task RelinkCurrentInterviewerToDeviceAsync()
        {
            this.IsInProgress = true;
            this.cancellationTokenSource = new CancellationTokenSource();
            try
            {
                await this.synchronizationService.LinkCurrentInterviewerToDeviceAsync(
                    credentials: new RestCredentials()
                    {
                        Login = this.userIdentityToRelink.Name,
                        Password = this.userIdentityToRelink.Password
                    },
                    token: this.cancellationTokenSource.Token);

                this.interviewersPlainStorage.Store(this.userIdentityToRelink);
                this.principal.SignIn(this.userIdentityToRelink.Name, this.userIdentityToRelink.Password, true);
                this.viewModelNavigationService.NavigateToDashboard();
            }
            catch (SynchronizationException ex)
            {
                this.ErrorMessage = ex.Message;
            }
            catch (Exception)
            {
                this.ErrorMessage = InterviewerUIResources.UnexpectedException;   
            }
            finally
            {
                this.IsInProgress = false;
            }
        }

        public void NavigateToPreviousViewModel()
        {
            this.cancellationTokenSource.Cancel();
            this.viewModelNavigationService.NavigateTo<FinishInstallationViewModel>(this.userIdentityToRelink);
        }
    }
}