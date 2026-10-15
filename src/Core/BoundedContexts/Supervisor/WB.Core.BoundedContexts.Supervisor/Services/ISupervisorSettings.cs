using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.BoundedContexts.Supervisor.Services
{
    public interface ISupervisorSettings : IEnumeratorSettings, IDeviceSettings
    {
        void SetShowLocationOnMap(bool showLocationOnMap);
        void SetCommunicationIntegrityValidationIgnore(bool ignore);
        string InterviewerApplicationsDirectory { get; }
        bool DownloadUpdatesForInterviewerApp { get; }
        void SetDownloadUpdatesForInterviewerApp(bool downloadUpdatesForInterviewerApp);
    }
}
