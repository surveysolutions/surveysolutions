namespace WB.Core.GenericSubdomains.ErrorReporting.Services
{
    public interface IErrorReportingSettings
    {
        string GetDeviceId();
        string GetClientRegistrationId();
    }
}
