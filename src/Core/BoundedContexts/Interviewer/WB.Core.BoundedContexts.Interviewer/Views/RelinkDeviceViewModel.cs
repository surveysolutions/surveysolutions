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
    public class RelinkDeviceViewModel : BaseViewModel<RelinkDeviceViewModelArg>
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly ISynchronizationService synchronizationService;
        private readonly IPlainStorage<InterviewerIdentity> interviewersPlainStorage;

        public RelinkDeviceViewModel(
            IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            ISynchronizationService synchronizationService,
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage)
            : base(principal, viewModelNavigationService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.synchronizationService = synchronizationService;
            this.interviewersPlainStorage = interviewersPlainStorage;
        }

        protected override bool IsAuthenticationRequired => false;

        private string errorMessage;
        public string ErrorMessage
        {
            get => this.errorMessage;
            set { this.errorMessage = value; RaisePropertyChanged(); }
        }

        private bool isInProgress;
        public bool IsInProgress
        {
            get => this.isInProgress;
            set { this.isInProgress = value; RaisePropertyChanged(); }
        }

        public IMvxCommand CancelCommand => new MvxAsyncCommand(this.NavigateToPreviousViewModel, () => !this.IsInProgress);

        public IMvxCommand NavigateToDiagnosticsPageCommand
            => new MvxCommand(() => this.viewModelNavigationService.NavigateToAsync<DiagnosticsViewModel>(),
                () => !this.IsInProgress);

        private IMvxAsyncCommand relinkCommand;
        public IMvxAsyncCommand RelinkCommand
        {
            get
            {
                return relinkCommand ?? (relinkCommand = new MvxAsyncCommand(this.RelinkCurrentInterviewerToDeviceAsync, () => !this.IsInProgress));
            }
        }

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private InterviewerIdentity userIdentityToRelink;

        public override void Prepare(RelinkDeviceViewModelArg parameter)
        {
            this.userIdentityToRelink = parameter.Identity;
        }

        private async Task RelinkCurrentInterviewerToDeviceAsync()
        {
            this.IsInProgress = true;
            this.cancellationTokenSource = new CancellationTokenSource();
            try
            {
                await this.synchronizationService.LinkCurrentInterviewerToDeviceAsync(
                    credentials: new RestCredentials
                    {
                        Login = this.userIdentityToRelink.Name,
                        Token = this.userIdentityToRelink.Token
                    },
                    token: this.cancellationTokenSource.Token).ConfigureAwait(false);

                this.interviewersPlainStorage.Store(this.userIdentityToRelink);
                this.Principal.SignIn(this.userIdentityToRelink.Id, true);
                await this.viewModelNavigationService.NavigateToDashboardAsync();
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

        public Task NavigateToPreviousViewModel()
        {
            this.cancellationTokenSource.Cancel();
            return this.viewModelNavigationService.NavigateToAsync<FinishInstallationViewModel, FinishInstallationViewModelArg>(new FinishInstallationViewModelArg(this.userIdentityToRelink));
        }
    }
}
