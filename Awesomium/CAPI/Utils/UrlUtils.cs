using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Browsing.CAPI.Properties;
using Common.Utils;

namespace Browsing.CAPI.Utils
{
    public class UrlUtils : IUrlUtils
    {
        public  string GetDefaultUrl()
        {
            return Settings.Default.DefaultUrl;
        }

        public string GetEnpointUrl()
        {
            return Settings.Default.EndpointExportPath;
        }

        public string GetLoginUrl()
        {
            return GetDefaultUrl() + Settings.Default.LoginPath;
        }
        
        public  string GetLoggedStatusCheckUrl()
        {
            return string.Format("{0}{1}", GetDefaultUrl(), Settings.Default.LoggedStatusCheckPath);
        }

        public string GetLoginCapabilitiesCheckUrl()
        {
            return string.Format("{0}{1}", GetDefaultUrl(), Settings.Default.LoginCapabilitiesCheckPath);
        }

        public  string GetPushUrl(Guid clientId)
        {
            return string.Format("{0}{1}?url={2}&syncKey={3}", GetDefaultUrl(), Settings.Default.NetworkLocalExportPath, GetEnpointUrl(),
                clientId);
        }
        
        public  string GetPullUrl(Guid clientId)
        {
            return string.Format("{0}{1}?url={2}&syncKey={3}", GetDefaultUrl(), Settings.Default.NetworkLocalImportPath, GetEnpointUrl(), clientId);
        }
        
        public string GetPushCheckStateUrl(Guid processid)
        {
            return string.Format("{0}{1}?id={2}", GetDefaultUrl(), Settings.Default.NetworkCheckStatePath, processid); 
        }
        
        public string GetUsbPushUrl(Guid clientId)
        {
            return string.Format("{0}{1}?syncKey={2}", GetDefaultUrl(), Settings.Default.UsbExportPath, clientId);
        }

        public string GetUsbPullUrl(Guid clientId)
        {
            return string.Format("{0}{1}", GetDefaultUrl(), Settings.Default.UsbImportPath);
        }

        public string GetCheckPushPrerequisitesUrl()
        {
            return string.Format("{0}{1}", GetDefaultUrl(), Settings.Default.CheckEventPath);
        }

        public string GetWhoIsCurrentUserUrl()
        {
            return string.Format("{0}{1}", GetDefaultUrl(), Settings.Default.CurrentUserPath);
        }

        public string GetRegistrationUrl()
        {
            return string.Format("{0}{1}", GetDefaultUrl(), Settings.Default.SupervisorRegistrationLocalPath);
        }

        public string GetAuthorizationUrl()
        {
            return string.Format("{0}{1}?url={2}{3}", GetDefaultUrl(), Settings.Default.AuthServiceLocalPath, GetEnpointUrl(), Settings.Default.SupervisorServicePath);
        }

        public string GetCheckConfirmedAuthorizationUrl(Guid registrationId)
        {
            return string.Format("{0}{1}?url={2}{3}&id={4}", GetDefaultUrl(), Settings.Default.CheckAuthConfirmedPath, GetEnpointUrl(), Settings.Default.SupervisorServicePath, registrationId);
        }

        public string GetEndProcessUrl(Guid syncProcessId)
        {
            return string.Format("{0}{1}?id={2}", GetDefaultUrl(), Settings.Default.EndProcessCapiPath, syncProcessId);
        }

        public string GetPushStatisticUrl(Guid syncProcessId)
        {
            return string.Format("{0}{1}?id={2}", GetDefaultUrl(), Settings.Default.PushStatisticCapiPath, syncProcessId);
        }

        public string GetPullStatisticUrl(Guid syncProcessId)
        {
            return string.Format("{0}{1}?id={2}", GetDefaultUrl(), Settings.Default.PullStatisticCapiPath, syncProcessId);
        }
     
        public string GetAuthorizedIDsUrl(Guid tabletId)
        {
            return string.Format("{0}{1}?tabletId={2}", GetDefaultUrl(), Settings.Default.AuthorizedIDsPath, tabletId);
        }

        public string GetAuthServiceUrl()
        {
            throw new NotImplementedException();
        }
    }
}
