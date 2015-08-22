using System;

namespace WB.Core.BoundedContexts.Interviewer.ErrorReporting.Services
{
    public interface IErrorReportingSettings
    {
        string GetDeviceId();
        Guid GetClientRegistrationId();
    }
}
