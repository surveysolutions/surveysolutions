using System;

namespace WB.UI.Capi.Settings
{
    public interface IInterviewerSettings
    {
        string GetDeviceId();
        Guid GetInstallationId();
        Guid? GetClientRegistrationId();
        string GetLastReceivedPackageId();
        string GetSyncAddressPoint();
        string GetApplicationVersionName();
        int GetApplicationVersionCode();
        string GetOperatingSystemVersion();
        void SetClientRegistrationId(Guid? clientRegistrationId);
        void SetLastReceivedPackageId(string lastReceivedPackageId);
        void SetSyncAddressPoint(string syncAddressPoint);
    }
}
