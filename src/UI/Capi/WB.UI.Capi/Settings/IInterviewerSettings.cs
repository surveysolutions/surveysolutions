namespace WB.UI.Capi.Settings
{
    public interface IInterviewerSettings
    {
        string GetDeviceId();
        string GetInstallationId();
        string GetClientRegistrationId();
        string GetLastReceivedPackageId();
        string GetSyncAddressPoint();
        string GetApplicationVersionName();
        int GetApplicationVersionCode();
        string GetOperatingSystemVersion();
        void SetClientRegistrationId(string clientRegistrationId);
        void SetLastReceivedPackageId(string lastReceivedPackageId);
        void SetSyncAddressPoint(string syncAddressPoint);
    }
}
