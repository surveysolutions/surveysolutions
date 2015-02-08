using System;

namespace WB.Core.BoundedContexts.Capi.Services
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

        [Obsolete]
        void SetLastReceivedPackageId(string lastReceivedPackageId);

        void SetSyncAddressPoint(string syncAddressPoint);
    }
}
