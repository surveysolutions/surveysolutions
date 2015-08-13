using System;

namespace WB.Core.BoundedContexts.Capi.ErrorReporting.Services
{
    public interface IErrorReportingSettings
    {
        string GetDeviceId();
        Guid GetClientRegistrationId();
    }
}
