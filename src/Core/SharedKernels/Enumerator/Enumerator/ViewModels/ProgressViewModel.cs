using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public abstract class ProgressViewModel<T> : BaseViewModel<T>
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
    }
}
