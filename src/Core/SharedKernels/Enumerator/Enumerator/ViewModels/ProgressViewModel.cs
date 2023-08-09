using MvvmCross.Commands;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public abstract class ProgressViewModel : BasePrincipalViewModel
    {
        protected ProgressViewModel(IPrincipal principal, 
            IViewModelNavigationService viewModelNavigationService) 
            : base(principal, viewModelNavigationService)
        {
        }

        private string progressDescription;

        public string ProgressDescription
        {
            get => this.progressDescription;
            set => SetProperty(ref this.progressDescription, value);
        }

        private string operationDescription;

        public string OperationDescription
        {
            get => this.operationDescription;
            set => SetProperty(ref this.operationDescription, value);
        }

        private string questionnaireTitle;

        public string QuestionnaireTitle
        {
            get => this.questionnaireTitle;
            set => SetProperty(ref this.questionnaireTitle, value);
        }

        private int progress = 0;

        public int Progress
        {
            get => this.progress;
            set => SetProperty(ref this.progress, value);
        }

        private bool isIndeterminate = true;

        public bool IsIndeterminate
        {
            get => this.isIndeterminate;
            set => SetProperty(ref this.isIndeterminate, value);
        }

        public IMvxCommand NavigateToDashboardCommand => new MvxAsyncCommand(async () => 
            await this.ViewModelNavigationService.NavigateToDashboardAsync());

        public IMvxCommand SignOutCommand => 
            new MvxAsyncCommand(this.ViewModelNavigationService.SignOutAndNavigateToLoginAsync);

        public IMvxCommand CancelLoadingCommand => new MvxCommand(this.CancelLoading);

        public virtual void CancelLoading()
        {
        }
    }
}
