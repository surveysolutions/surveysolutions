using System;

namespace Common.Utils
{
    public interface IUrlUtils
    {
        string GetDefaultUrl();
        string GetLoginUrl();
        string GetLoggedStatusCheckUrl();
        string GetLoginCapabilitiesCheckUrl();
        string GetPushUrl(Guid clientId);
        string GetPullUrl(Guid clientId);
        string GetPushCheckStateUrl(Guid processid);
        string GetEnpointUrl();
        string GetUsbPushUrl(Guid clientId);
        string GetUsbPullUrl(Guid clientId);
        string GetCheckPushPrerequisitesUrl();
        string GetWhoIsCurrentUserUrl();
        /// <summary>
        /// URL local endpoint to save registration data
        /// </summary>
        /// <returns></returns>
        string GetRegistrationUrl();
        /// <summary>
        /// URL local endpoint to send registration data for authorization
        /// </summary>
        /// <returns></returns>
        string GetAuthorizationUrl();
        /// <summary>
        /// URL local endpoint to check successfull registration
        /// </summary>
        /// <returns></returns>
        string GetCheckConfirmedAuthorizationUrl(Guid registrationId);
        string GetEndProcessUrl(Guid syncProcessId);
        string GetPushStatisticUrl(Guid syncProcessId);
        string GetPullStatisticUrl(Guid syncProcessId);
        string GetAuthorizedIDsUrl(Guid registratorId);
        string GetAuthServiceUrl();
    }
}