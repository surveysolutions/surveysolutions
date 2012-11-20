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
        
        public  string GetAuthentificationCheckUrl()
        {
            return string.Format("{0}{1}", GetDefaultUrl(), Settings.Default.AuthentificationCheckPath);
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

        public string GetCurrentUserGetUrl()
        {
            return string.Format("{0}{1}", GetDefaultUrl(), Settings.Default.GetCurrentUserPath);
        }

        public string GetRegistrationCapiPath()
        {
            return string.Format("{0}{1}", GetDefaultUrl(), Settings.Default.GetRegistrationCapiPath);
        }

        public string GetEndProcessUrl(Guid id)
        {
            return string.Format("{0}{1}?id={2}", GetDefaultUrl(), Settings.Default.GetEndProcessCapiPath, id);
        }

        public string GetPushStatisticUrl()
        {
            return string.Format("{0}{1}", GetDefaultUrl(), Settings.Default.GetPushStatisticCapiPath);
        }

        public string GetPullStatisticUrl()
        {
            return string.Format("{0}{1}", GetDefaultUrl(), Settings.Default.GetPullStatisticCapiPath);
        }
        public string GetAuthorizedIDsUrl(Guid registratorId)
        {
            return string.Empty;
        }
    }
}
