using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class DiagnosticsViewModel : BaseViewModel
    {
        private readonly IPrincipal principal;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IInterviewerSettings interviewerSettings;

        public DiagnosticsViewModel(IPrincipal principal, IViewModelNavigationService viewModelNavigationService, IInterviewerSettings interviewerSettings)
        {
            this.principal = principal;
            this.viewModelNavigationService = viewModelNavigationService;
            this.interviewerSettings = interviewerSettings;
        }

        public void Init()
        {
            Version = this.interviewerSettings.GetApplicationVersionName();
            IsRestoreVisible = false;
        }

        public bool IsRestoreVisible
        {
            get { return this.isRestoreVisible; }
            set { this.isRestoreVisible = value; this.RaisePropertyChanged(); }
        }
        private bool isRestoreVisible;
        public string Version { get; set; }
    }
}