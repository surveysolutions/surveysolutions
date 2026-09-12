using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    public class RelinkDeviceViewModel : BaseViewModel<RelinkDeviceViewModelArg>
    {
        private readonly ISynchronizationService synchronizationService;
        private readonly IAuditLogService auditLogService;
        private const string StateKey = "supervisorIdentity";
        private readonly ISupervisorPrincipal supervisorPrincipal;

        public RelinkDeviceViewModel(
            ISupervisorPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            ISynchronizationService synchronizationService,
            IAuditLogService auditLogService)
            : base(principal, viewModelNavigationService, false)
        {
            this.synchronizationService = synchronizationService;
            this.auditLogService = auditLogService;
            this.supervisorPrincipal = principal;
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
            set => SetProperty(ref this.isInProgress, value);
        }

        public IMvxAsyncCommand CancelCommand => new MvxAsyncCommand(this.NavigateToPreviousViewModel, () => !this.IsInProgress);

        public IMvxAsyncCommand NavigateToDiagnosticsPageCommand
            => new MvxAsyncCommand(
                () => this.ViewModelNavigationService.NavigateToAsync<DiagnosticsViewModel>(),
                () => !this.IsInProgress);

        public IMvxAsyncCommand RelinkCommand => new MvxAsyncCommand(this.RelinkCurrentSupervisorToDeviceAsync, () => !this.IsInProgress);

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private SupervisorIdentity userIdentityToRelink;

        public override void Prepare(RelinkDeviceViewModelArg parameter)
        {
            this.userIdentityToRelink = parameter.Identity;
        }

        protected override void SaveStateToBundle(IMvxBundle bundle)
        {
            base.SaveStateToBundle(bundle);
            bundle.Data[StateKey] = JsonConvert.SerializeObject(this.userIdentityToRelink);
        }

        protected override void ReloadFromBundle(IMvxBundle state)
        {
            base.ReloadFromBundle(state);
            if (state.Data.ContainsKey(StateKey))
            {
                this.userIdentityToRelink = JsonConvert.DeserializeObject<SupervisorIdentity>(state.Data[StateKey]);
            }
        }

        private async Task RelinkCurrentSupervisorToDeviceAsync()
        {
            if (this.userIdentityToRelink == null)
            {
                throw new ArgumentNullException(nameof(userIdentityToRelink));
            }
            this.IsInProgress = true;
            this.cancellationTokenSource = new CancellationTokenSource();
            try
            {
                await this.synchronizationService.LinkCurrentUserToDeviceAsync(
                    credentials: new RestCredentials
                    {
                        Login = this.userIdentityToRelink.Name,
                        Token = this.userIdentityToRelink.Token,
                        Workspace = this.userIdentityToRelink.Workspace
                    },
                    token: this.cancellationTokenSource.Token).ConfigureAwait(false);

                this.supervisorPrincipal.SaveSupervisor(this.userIdentityToRelink);
                this.supervisorPrincipal.SignIn(this.userIdentityToRelink.Id, true);
                auditLogService.WriteApplicationLevelRecord(new RelinkAuditLogEntity());
                await this.ViewModelNavigationService.NavigateToDashboardAsync();
            }
            catch (SynchronizationException ex)
            {
                this.ErrorMessage = ex.Message;
            }
            catch (OperationCanceledException) when (this.cancellationTokenSource.IsCancellationRequested)
            {
                // User-initiated cancellation (Back navigation) — no error message.
            }
            catch (Exception)
            {
                this.ErrorMessage = EnumeratorUIResources.UnexpectedException;
            }
            finally
            {
                this.IsInProgress = false;
            }
        }

        public Task NavigateToPreviousViewModel()
        {
            this.cancellationTokenSource.Cancel();
            return this.ViewModelNavigationService.NavigateToAsync<FinishInstallationViewModel, FinishInstallationViewModelArg>(
                new FinishInstallationViewModelArg(this.userIdentityToRelink?.Name));
        }    }
}
