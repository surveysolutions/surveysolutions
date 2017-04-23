using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Shared.Enumerator.CustomServices.AreaEditor
{
    public class AreaEditorViewModel : BaseViewModel
    {
        public AreaEditorViewModel(IPrincipal principal, IViewModelNavigationService viewModelNavigationService) 
            : base(principal, viewModelNavigationService)
        {
        }
        
        protected void Initialize(string area)
        {
            this.Area = area;
        }

        public string Area { set; get; }
    }
}