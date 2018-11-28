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
    }
}
