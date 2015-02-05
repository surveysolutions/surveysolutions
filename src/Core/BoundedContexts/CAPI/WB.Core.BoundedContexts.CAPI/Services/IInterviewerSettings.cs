using System;

namespace WB.Core.BoundedContexts.Capi.Services
{
    public interface IInterviewerSettings
    {
        string GetDeviceId();

        Guid GetInstallationId();

        Guid? GetClientRegistrationId();

        [Obsolete]
        string GetLastReceivedPackageId();

        string GetLastReceivedUserPackageId();
        
        string GetLastReceivedQuestionnairePackageId();

        string GetLastReceivedInterviewPackageId();

        string GetSyncAddressPoint();

        string GetApplicationVersionName();

        int GetApplicationVersionCode();

        string GetOperatingSystemVersion();

        void SetClientRegistrationId(Guid? clientRegistrationId);

        [Obsolete]
        void SetLastReceivedPackageId(string lastReceivedPackageId);

        void SetLastReceivedUserPackageId(string lastReceivedUserPackageId);

        void SetLastReceivedQuestionnairePackageId(string lastReceivedQuestionnairePackageId);

        void SetLastReceivedInterviewPackageId(string lastReceivedInterviewPackageId);

        void SetSyncAddressPoint(string syncAddressPoint);
    }
}
