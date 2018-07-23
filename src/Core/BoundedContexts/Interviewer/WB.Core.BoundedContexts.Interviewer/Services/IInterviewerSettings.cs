using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IInterviewerSettings : IEnumeratorSettings, IDeviceSettings
    {
        void SetVibrateOnError(bool vibrate);
        void SetShowLocationOnMap(bool showLocationOnMap);
        void SetAllowSyncWithHq(bool allowSyncWithHq);
        bool AllowSyncWithHq { get; }
    }
}
