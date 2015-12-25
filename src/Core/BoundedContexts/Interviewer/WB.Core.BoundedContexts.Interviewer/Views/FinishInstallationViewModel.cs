using System;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class FinishInstallationViewModel : BaseViewModel
    {
        readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IPrincipal principal;
        private readonly IPasswordHasher passwordHasher;
        private readonly IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage;
        private readonly IInterviewerSettings interviewerSettings;
        private readonly ISynchronizationService synchronizationService;
        private readonly ILogger logger;
        private CancellationTokenSource cancellationTokenSource;

        public FinishInstallationViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            IPasswordHasher passwordHasher,
            IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage,
            IInterviewerSettings interviewerSettings,
            ISynchronizationService synchronizationService,
            ILogger logger)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.principal = principal;
            this.passwordHasher = passwordHasher;
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.interviewerSettings = interviewerSettings;
            this.synchronizationService = synchronizationService;
            this.logger = logger;
        }

        private string endpoint;
        public string Endpoint
        {
            get { return this.endpoint; }
            set { this.endpoint = value; RaisePropertyChanged(); }
        }

        private string userName;
        public string UserName
        {
            get { return this.userName; }
            set { this.userName = value; RaisePropertyChanged(); }
        }

        private string password;
        public string Password
        {
            get { return this.password; }
            set { this.password = value; RaisePropertyChanged(); }
        }

        private bool isEndpointValid;
        public bool IsEndpointValid
        {
            get { return this.isEndpointValid; }
            set { this.isEndpointValid = value; RaisePropertyChanged(); }
        }

        private bool isUserValid;
        public bool IsUserValid
        {
            get { return this.isUserValid; }
            set { this.isUserValid = value; RaisePropertyChanged(); }
        }

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

        private IMvxCommand signInCommand;
        public IMvxCommand SignInCommand
        {
            get { return this.signInCommand ?? (this.signInCommand = new MvxCommand(async () => await this.SignInAsync(), () => !IsInProgress)); }
        }

        public IMvxCommand NavigateToDiagnosticsPageCommand
        {
            get { return new MvxCommand(async () => await this.viewModelNavigationService.NavigateToAsync<DiagnosticsViewModel>()); }
        }

        public void Init(InterviewerIdentity userIdentity)
        {
            this.IsUserValid = true;
            this.IsEndpointValid = true;
            this.Endpoint = this.interviewerSettings.Endpoint;

            this.UserName = userIdentity.Name;
        }

        public void RefreshEndpoint()
        {
            this.Endpoint = this.interviewerSettings.Endpoint;
        }

        private async Task SignInAsync()
        {
            this.IsUserValid = true;
            this.IsEndpointValid = true;
            bool isNeedNavigateToRelinkPage = false;

            await this.interviewerSettings.SetEndpointAsync(this.Endpoint);

            var restCredentials = new RestCredentials
            {
                Login = this.UserName,
                Password = this.passwordHasher.Hash(this.Password)
            };

            cancellationTokenSource = new CancellationTokenSource();
            this.IsInProgress = true;
            InterviewerIdentity interviewerIdentity = null;
            try
            {
                var interviewer = await this.synchronizationService.GetInterviewerAsync(restCredentials, token: cancellationTokenSource.Token);
                interviewerIdentity = new InterviewerIdentity
                {
                    Id = interviewer.Id.FormatGuid(),
                    UserId = interviewer.Id,
                    SupervisorId = interviewer.SupervisorId,
                    Name = restCredentials.Login,
                    Password = restCredentials.Password
                };
                if (!await this.synchronizationService.HasCurrentInterviewerDeviceAsync(credentials: restCredentials, token: cancellationTokenSource.Token))
                {
                    await this.synchronizationService.LinkCurrentInterviewerToDeviceAsync(credentials: restCredentials, token: cancellationTokenSource.Token);
                }

                await this.synchronizationService.CanSynchronizeAsync(credentials: restCredentials, token: cancellationTokenSource.Token);
                
                await this.interviewersPlainStorage.StoreAsync(interviewerIdentity);

                this.principal.SignIn(restCredentials.Login, restCredentials.Password, true);
                await this.viewModelNavigationService.NavigateToDashboardAsync();
            }
            catch (SynchronizationException ex)
            {
                switch (ex.Type)
                {
                    case SynchronizationExceptionType.HostUnreachable:
                    case SynchronizationExceptionType.InvalidUrl:
                    case SynchronizationExceptionType.ServiceUnavailable:
                        this.IsEndpointValid = false;
                        break;
                    case SynchronizationExceptionType.UserIsNotInterviewer:
                    case SynchronizationExceptionType.UserLocked:
                    case SynchronizationExceptionType.UserNotApproved:
                    case SynchronizationExceptionType.Unauthorized:
                        this.IsUserValid = false;
                        break;
                    case SynchronizationExceptionType.UserLinkedToAnotherDevice:
                        isNeedNavigateToRelinkPage = true;
                        break;
                }
                this.ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                this.ErrorMessage = InterviewerUIResources.UnexpectedException;
                this.logger.Error("Login view model. Unexpected exception", ex);
            }
            finally
            {
                this.IsInProgress = false;
                cancellationTokenSource = null;
            }

            if (isNeedNavigateToRelinkPage)
                await this.viewModelNavigationService.NavigateToAsync<RelinkDeviceViewModel>(interviewerIdentity);
        }

        public void CancellInProgressTask()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
            }
        }
    }
}