using System;

using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.GenericSubdomains.ErrorReporting.Services;

namespace WB.UI.Capi.Settings
{
    internal class ErrorReportingSettings : IErrorReportingSettings
    {
        private readonly IInterviewerSettings interviewerSettings;

        public ErrorReportingSettings(IInterviewerSettings interviewerSettings)
        {
            this.interviewerSettings = interviewerSettings;
        }

        public string GetDeviceId()
        {
            return this.interviewerSettings.GetDeviceId();
        }

        public Guid GetClientRegistrationId()
        {
            if (!this.interviewerSettings.GetClientRegistrationId().HasValue)
                throw new NullReferenceException("ClientRegistrationId is not registered. Synchronize application with server first.");

            return this.interviewerSettings.GetClientRegistrationId().Value;
        }
    }
}