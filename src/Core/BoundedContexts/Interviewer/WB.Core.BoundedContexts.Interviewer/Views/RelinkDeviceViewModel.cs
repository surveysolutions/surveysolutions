using System;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class RelinkDeviceViewModel : BaseViewModel<RelinkDeviceViewModelArg>
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly ISynchronizationService synchronizationService;
        private readonly IPlainStorage<InterviewerIdentity> interviewersPlainStorage;
        private readonly IAuditLogService auditLogService;
        private const string StateKey = "interviewerIdentity";

        public RelinkDeviceViewModel(
            IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            ISynchronizationService synchronizationService,
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage,
            IAuditLogService auditLogService)
            : base(principal, viewModelNavigationService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.synchronizationService = synchronizationService;
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.auditLogService = auditLogService;
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

        public IMvxAsyncCommand CancelCommand => new MvxAsyncCommand(this.NavigateToPreviousViewModel, () => !this.IsInProgress);

        public IMvxAsyncCommand NavigateToDiagnosticsPageCommand
            => new MvxAsyncCommand(this.viewModelNavigationService.NavigateToAsync<DiagnosticsViewModel>,
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
                this.userIdentityToRelink = JsonConvert.DeserializeObject<InterviewerIdentity>(state.Data[StateKey]);
            }
        }

        private async Task RelinkCurrentInterviewerToDeviceAsync()
        {
            this.IsInProgress = true;
            this.cancellationTokenSource = new CancellationTokenSource();
            try
            {
                await this.synchronizationService.LinkCurrentUserToDeviceAsync(
                    credentials: new RestCredentials
                    {
                        Login = this.userIdentityToRelink.Name,
                        Token = this.userIdentityToRelink.Token
                    },
                    token: this.cancellationTokenSource.Token).ConfigureAwait(false);

                this.interviewersPlainStorage.Store(this.userIdentityToRelink);
                this.Principal.SignIn(this.userIdentityToRelink.Id, true);
                auditLogService.Write( new RelinkAuditLogEntity());
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
            return this.viewModelNavigationService.NavigateToAsync<FinishInstallationViewModel, FinishInstallationViewModelArg>(
                new FinishInstallationViewModelArg(this.userIdentityToRelink.Name));
        }
    }
}
