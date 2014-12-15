using System;

namespace WB.Core.GenericSubdomains.ErrorReporting.Services
{
    public interface IErrorReportingSettings
    {
        string GetDeviceId();
        Guid GetClientRegistrationId();
    }
}
