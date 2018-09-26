using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.UI.Supervisor.Views
{
    public class ApplicationSettingsView : EnumeratorSettingsView
    {
        public bool ShowLocationOnMap { get; set; }
        public bool DownloadUpdatesForInterviewerApp { get; set; }
    }
}
