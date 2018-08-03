using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class ApplicationSettingsView : EnumeratorSettingsView
    {
        public int GpsResponseTimeoutInSec { get; set; }
        public double? GpsDesiredAccuracy { get; set; }
        public bool? VibrateOnError { get; set; }
        public bool? ShowLocationOnMap { get; set; }
        public bool? AllowSyncWithHq { get; set; }
        public bool? IsOfflineSynchronizationDone { get; set; }
    }
}
