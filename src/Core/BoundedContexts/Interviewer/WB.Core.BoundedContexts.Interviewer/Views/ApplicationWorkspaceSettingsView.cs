using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class ApplicationWorkspaceSettingsView : EnumeratorWorkspaceSettingsView
    {
        public bool? AllowSyncWithHq { get; set; }
        public bool? IsOfflineSynchronizationDone { get; set; }
    }
}