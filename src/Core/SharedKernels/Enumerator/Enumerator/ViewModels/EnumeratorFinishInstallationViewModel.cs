using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public abstract class EnumeratorFinishInstallationViewModel : BaseViewModel<FinishInstallationViewModelArg>
    {
        private readonly IDeviceSettings deviceSettings;
        private readonly ISynchronizationService synchronizationService;
        private readonly ILogger logger;
        private CancellationTokenSource cancellationTokenSource;
        private readonly IUserInteractionService userInteractionService;
        private readonly IAuditLogService auditLogService;
        private readonly IDeviceInformationService deviceInformationService;
        private readonly IWorkspaceService workspaceService;
        private const string StateKey = "identity";
        private readonly IQRBarcodeScanService qrBarcodeScanService;
        private readonly ISerializer serializer;

        protected EnumeratorFinishInstallationViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            IDeviceSettings deviceSettings,
            ISynchronizationService synchronizationService,
            ILogger logger,
            IQRBarcodeScanService qrBarcodeScanService,
            ISerializer serializer,
            IUserInteractionService userInteractionService,
            IAuditLogService auditLogService,
            IDeviceInformationService deviceInformationService,
            IWorkspaceService workspaceService) 
                :base(principal, viewModelNavigationService, false)
        {
            this.deviceSettings = deviceSettings;
            this.synchronizationService = synchronizationService;
            this.logger = logger;
            this.userInteractionService = userInteractionService;
            this.auditLogService = auditLogService;
            this.deviceInformationService = deviceInformationService;
            this.workspaceService = workspaceService;

            this.qrBarcodeScanService = qrBarcodeScanService;
            this.serializer = serializer;
        }
        
        private string endpoint;
        public string Endpoint
        {
            get => this.endpoint;
            set { this.endpoint = value; RaisePropertyChanged(); }
        }

        private string userName;
        public string UserName
        {
            get => this.userName;
            set { this.userName = value; RaisePropertyChanged(); }
        }

        private string password;
        public string Password
        {
            get => this.password;
            set
            {
                SetProperty(ref this.password, value);
                this.PasswordError = null;
            }
        }

        private bool isUserValid;
        public bool IsUserValid
        {
            get => this.isUserValid;
            set => SetProperty(ref this.isUserValid, value);
        }

        private string errorMessage;
        public string ErrorMessage
        {
            get => this.errorMessage;
            set => SetProperty(ref this.errorMessage, value);
        }

        private bool isInProgress;
        public bool IsInProgress
        {
            get => this.isInProgress;
            set { this.isInProgress = value; RaisePropertyChanged(); }
        }

        private IMvxAsyncCommand signInCommand;
        public IMvxAsyncCommand SignInCommand => this.signInCommand ??= new MvxAsyncCommand(this.SignInAsync, () => !IsInProgress);

        public IMvxAsyncCommand NavigateToDiagnosticsPageCommand => new MvxAsyncCommand(this.ViewModelNavigationService.NavigateToAsync<DiagnosticsViewModel>);

        public override void Prepare(FinishInstallationViewModelArg parameter)
        {
            this.UserName = parameter.UserName;
        }

        public override async Task Initialize()
        {
            await base.Initialize().ConfigureAwait(false);

            this.IsUserValid = true;
            this.Endpoint =  this.deviceSettings.Endpoint;

#if DEBUG
            this.Endpoint = "http://192.168.50.213:5001";
            this.UserName = "in1sv3";
            this.Password = "Qwerty1234";
#endif
        }

        protected override void SaveStateToBundle(IMvxBundle bundle)
        {
            base.SaveStateToBundle(bundle);
            if (this.UserName != null)
            {
                bundle.Data[StateKey] = this.userName;
            }
        }

        protected override void ReloadFromBundle(IMvxBundle state)
        {
            base.ReloadFromBundle(state);
            if (state.Data.ContainsKey(StateKey))
            {
                this.UserName = state.Data[StateKey];
            }
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

#pragma warning disable 4014
            RefreshEndpoint(); // It is handing if awaited
#pragma warning restore 4014
        }

        public async Task RefreshEndpoint()
        {
            var settingsEndpoint = this.deviceSettings.Endpoint;
            if (!string.IsNullOrEmpty(settingsEndpoint) && !string.Equals(settingsEndpoint, this.endpoint, StringComparison.OrdinalIgnoreCase))
            {
                var message = string.Format(EnumeratorUIResources.FinishInstallation_EndpointDiffers,  this.Endpoint, settingsEndpoint);
                if (await this.userInteractionService.ConfirmAsync(message, isHtml: false).ConfigureAwait(false))
                {
                    this.Endpoint = settingsEndpoint;
                }
            }
        }

        private async Task SignInAsync()
        {
            this.IsUserValid = true;
            this.ErrorMessage = null;
            this.EndpointValidationError = null;

            if (this.Endpoint?.StartsWith("@") == true)
            {
                this.Endpoint = $"https://{this.Endpoint.Substring(1).Trim()}.mysurvey.solutions";
            }

            this.deviceSettings.SetEndpoint(this.Endpoint.Trim());

            var restCredentials = new RestCredentials
            {
                Login = this.userName
            };

            cancellationTokenSource = new CancellationTokenSource();
            this.IsInProgress = true;
            try
            {
                if (string.IsNullOrWhiteSpace(UserName))
                {
                    throw new SynchronizationException(SynchronizationExceptionType.Unauthorized, EnumeratorUIResources.Login_WrongPassword);
                }

                var authToken = await this.synchronizationService.LoginAsync(new LogonInfo
                {
                    Username = this.UserName,
                    Password = this.Password
                }, restCredentials, cancellationTokenSource.Token).ConfigureAwait(false);

                restCredentials.Token = authToken;

                var workspaces = await GetUserWorkspaces(restCredentials, cancellationTokenSource.Token);
                restCredentials.Workspace = workspaces.First().Name;
                
                if (!await this.synchronizationService.HasCurrentUserDeviceAsync(credentials: restCredentials, token: cancellationTokenSource.Token).ConfigureAwait(false))
                {
                    await this.synchronizationService.LinkCurrentUserToDeviceAsync(credentials: restCredentials, token: cancellationTokenSource.Token).ConfigureAwait(false);
                }

                await this.synchronizationService.CanSynchronizeAsync(credentials: restCredentials, token: cancellationTokenSource.Token).ConfigureAwait(false);

                SaveWorkspaces(workspaces);
                await this.SaveUserToLocalStorageAsync(restCredentials, cancellationTokenSource.Token);

                this.Principal.SignIn(restCredentials.Login, this.Password, true);

                this.auditLogService.Write(new FinishInstallationAuditLogEntity(this.Endpoint));
                this.auditLogService.Write(new LoginAuditLogEntity(this.UserName));

                await this.ViewModelNavigationService.NavigateToDashboardAsync();
            }
            catch (SynchronizationException ex)
            {
                this.ErrorMessage = ex.Message;

                switch (ex.Type)
                {
                    case SynchronizationExceptionType.HostUnreachable:
                    case SynchronizationExceptionType.InvalidUrl:
                    case SynchronizationExceptionType.ServiceUnavailable:
                        this.EndpointValidationError = EnumeratorUIResources.InvalidEndpointShort;
                        break;
                    case SynchronizationExceptionType.UserIsNotInterviewer:
                    case SynchronizationExceptionType.UserLocked:
                    case SynchronizationExceptionType.UserNotApproved:
                    case SynchronizationExceptionType.Unauthorized:
                        this.IsUserValid = false;
                        this.PasswordError = EnumeratorUIResources.Login_WrongPassword;
                        break;
                    case SynchronizationExceptionType.UserLinkedToAnotherDevice:
                        await this.RelinkUserToAnotherDeviceAsync(restCredentials, cancellationTokenSource.Token);
                        break;
                    
                    case SynchronizationExceptionType.UpgradeRequired:
                        var targetVersionObj = ex.Data["target-version"];
                        if (targetVersionObj != null && targetVersionObj is string targetVersion)
                        {
                            var appVersionName = deviceInformationService.GetApplicationVersionName();
                            ErrorMessage = GetRequiredUpdateMessage(targetVersion, appVersionName);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                this.ErrorMessage = EnumeratorUIResources.UnexpectedException;
                this.logger.Error("Finish installation view model. Unexpected exception", ex);
            }
            finally
            {
                this.IsInProgress = false;
                cancellationTokenSource = null;
            }
        }

        private void SaveWorkspaces(List<UserWorkspaceApiView> workspaces)
        {
            workspaceService.Save(workspaces.Select(w => new WorkspaceView()
            {
                Id = w.Name,
                DisplayName = w.DisplayName,
                Disabled = w.Disabled,
                SupervisorId = w.SupervisorId,
            }).ToArray());
        }

        public string PasswordError
        {
            get => passwordError;
            set => SetProperty(ref passwordError, value);
        }

        public string EndpointValidationError
        {
            get => endpointValidationError;
            set => SetProperty(ref endpointValidationError, value);
        }

        protected abstract string GetRequiredUpdateMessage(string targetVersion, string appVersion);
        protected abstract Task RelinkUserToAnotherDeviceAsync(RestCredentials credentials, CancellationToken token);
        protected abstract Task SaveUserToLocalStorageAsync(RestCredentials credentials, CancellationToken token);
        
        protected abstract Task<List<UserWorkspaceApiView>> GetUserWorkspaces(RestCredentials credentials,
            CancellationToken token);

        public void CancellInProgressTask() => this.cancellationTokenSource?.Cancel();

        private IMvxAsyncCommand scanAsyncCommand;
        private string endpointValidationError;
        private string passwordError;

        public IMvxAsyncCommand ScanCommand
        {
            get { return this.scanAsyncCommand ?? (this.scanAsyncCommand = new MvxAsyncCommand(this.ScanAsync, () => !IsInProgress)); }
        }

        protected async Task ScanAsync()
        {
            this.IsInProgress = true;

            try
            {
                var scanCode = await this.qrBarcodeScanService.ScanAsync();

                if (scanCode?.Code != null)
                {
                    // Try parse scanned barcode as url
                    // For barcode on download page which have a link to interviewer apk
                    Uri.TryCreate(scanCode.Code, UriKind.Absolute, out var scannedUrl);
                    if (scannedUrl != null)
                        this.Endpoint = scannedUrl.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
                    else
                    {
                        var finishInfo = this.serializer.Deserialize<FinishInstallationInfo>(scanCode.Code);

                        this.Endpoint = finishInfo.Url;
                        this.UserName = finishInfo.Login;
                    }
                }
            }
            catch (Exception e)
            {
                this.logger.Error("Qrbarcode reader error: ", e);
                this.ErrorMessage = EnumeratorUIResources.FinishInstallation_QrBarcodeReaderErrorMessage;
            }
            finally
            {
                this.IsInProgress = false;
            }
        }
    }
}
