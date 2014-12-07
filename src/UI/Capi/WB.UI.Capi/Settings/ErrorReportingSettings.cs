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

        public string GetClientRegistrationId()
        {
            return this.interviewerSettings.GetClientRegistrationId();
        }
    }
}