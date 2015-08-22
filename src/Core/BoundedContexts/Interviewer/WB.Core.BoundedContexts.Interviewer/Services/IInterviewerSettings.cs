using System;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IInterviewerSettings
    {
        string GetDeviceId();

        Guid GetInstallationId();

        Guid? GetClientRegistrationId();

        string GetSyncAddressPoint();

        string GetApplicationVersionName();

        int GetApplicationVersionCode();

        string GetOperatingSystemVersion();

        void SetClientRegistrationId(Guid? clientRegistrationId);

        void SetSyncAddressPoint(string syncAddressPoint);
    }
}
