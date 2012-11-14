using System;

namespace Common.Utils
{
    public interface IUrlUtils
    {
        string GetDefaultUrl();
        string GetLoginUrl();
        string GetAuthentificationCheckUrl();
        string GetLoginCapabilitiesCheckUrl();
        string GetPushUrl(Guid clientId);
        string GetPullUrl(Guid clientId);
        string GetPushCheckStateUrl(Guid processid);
        string GetEnpointUrl();
        string GetUsbPushUrl(Guid clientId);
        string GetUsbPullUrl(Guid clientId);
        string GetCheckPushPrerequisitesUrl();
        string GetCurrentUserGetUrl();
        string GetRegistrationCapiPath();
        string GetEndProcessUrl(Guid id);
        string GetAuthorizedIDsUrl(Guid registratorId);
    }
}