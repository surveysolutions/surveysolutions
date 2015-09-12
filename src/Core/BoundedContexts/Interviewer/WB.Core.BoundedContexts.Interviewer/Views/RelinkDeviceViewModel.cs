using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class RelinkDeviceViewModel : BaseViewModel
    {
        private readonly ICapiCleanUpService cleanUpExecutor;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly ISynchronizationService synchronizationService;
        private readonly IAsyncPlainStorage<InterviewerIdentity> interviewerAsyncPlainStorage;
        private readonly IUserIdentity userIdentity;

        public RelinkDeviceViewModel(
            ICapiCleanUpService cleanUpExecutor, 
            IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            ISynchronizationService synchronizationService,
            IAsyncPlainStorage<InterviewerIdentity> interviewerAsyncPlainStorage)
        {
            this.cleanUpExecutor = cleanUpExecutor;
            this.viewModelNavigationService = viewModelNavigationService;
            this.synchronizationService = synchronizationService;
            this.interviewerAsyncPlainStorage = interviewerAsyncPlainStorage;
            this.userIdentity = principal.CurrentUserIdentity;
        }

        public IMvxCommand CancelCommand
        {
            get { return new MvxCommand(this.ReturnBack); }
        }

        public IMvxCommand RelinkCommand
        {
            get { return new MvxCommand(async () => await this.RelinkCurrentInterviewerToDeviceAsync()); }
        }

        private bool shouldReturnToFinishInstallation;

        public void Init(bool redirectedFromFinishInstallation)
        {
            this.shouldReturnToFinishInstallation = redirectedFromFinishInstallation;
        }

        private void ReturnBack()
        {
            if (shouldReturnToFinishInstallation)
            {
                var currentInterviver = this.interviewerAsyncPlainStorage.Query(interviewers => interviewers.FirstOrDefault());
                if (currentInterviver != null)
                {
                    this.interviewerAsyncPlainStorage.RemoveAsync(currentInterviver.Id);   
                }

                this.viewModelNavigationService.NavigateTo<LoginViewModel>();

            }
            else
                this.viewModelNavigationService.NavigateToDashboard();
        }

        private async Task RelinkCurrentInterviewerToDeviceAsync()
        {
            await this.synchronizationService.LinkCurrentInterviewerToDeviceAsync(token: default(CancellationToken));
            this.cleanUpExecutor.DeleteAllInterviewsForUser(this.userIdentity.UserId);
            this.viewModelNavigationService.NavigateToDashboard();
        }

        public override void NavigateToPreviousViewModel()
        {
            this.ReturnBack();
        }
    }
}