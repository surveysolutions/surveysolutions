using System;
using WB.Core.SharedKernels.Enumerator;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IInterviewerSettings : IEnumeratorSettings
    {
        string GetDeviceId();

        Guid GetInstallationId();

        Guid? GetClientRegistrationId();
        
        string GetApplicationVersionName();

        int GetApplicationVersionCode();

        string GetOperatingSystemVersion();

        void SetClientRegistrationId(Guid? clientRegistrationId);

        void SetSyncAddressPoint(string syncAddressPoint);
    }
}
