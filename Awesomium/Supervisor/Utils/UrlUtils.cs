using System;
using Common.Utils;
using Browsing.Supervisor.Properties;

namespace Browsing.Supervisor.Utils
{
    public class UrlUtils : IUrlUtils
    {
        public string GetDefaultUrl()
        {
            return Settings.Default.DefaultUrl;
        }

        public string GetLoginUrl()
        {
            return GetDefaultUrl() + Settings.Default.LoginPath;
        }

        public string GetAuthentificationCheckUrl()
        {
            return string.Format("{0}{1}", GetDefaultUrl(), Settings.Default.AuthentificationCheckPath);
        }

        public string GetLoginCapabilitiesCheckUrl()
        {
            return string.Format("{0}{1}", GetDefaultUrl(), Settings.Default.LoginCapabilitiesCheckPath);
        }

        public string GetEnpointUrl()
        {
            return Settings.Default.EndpointExportPath;
        }

        public string GetUsbPushUrl(Guid clientId)
        {
            throw new NotImplementedException();
        }

        public string GetCheckPushPrerequisitesUrl()
        {
            return string.Format("{0}{1}", GetDefaultUrl(), Settings.Default.CheckEventPath);
        }

        public string GetPushUrl(Guid clientId)
        {
            throw new NotImplementedException();
        }

        public string GetPullUrl(Guid clientId)
        {
            throw new NotImplementedException();
        }

        public string GetPushCheckStateUrl(Guid processid)
        {
            throw new NotImplementedException();
        }

        public string GetUsbPullUrl(Guid clientId)
        {
            return string.Format("{0}{1}?syncKey={2}", GetDefaultUrl(), Settings.Default.UsbImportPath, clientId);
        }

        public string GetCurrentUserGetUrl()
        {
            return string.Format("{0}{1}", GetDefaultUrl(), Settings.Default.GetCurrentUserPath);
        }

        public string GetRegistrationCapiPath()
        {
            return string.Format("{0}{1}", GetDefaultUrl(), Settings.Default.GetRegistrationSupervisorPath);
        }

        public string GetEndProcessUrl(Guid id)
        {
            //return string.Format("{0}{1}?id={2}", GetDefaultUrl(), Settings.Default.GetEndProcessCapiPath, id);
            return String.Empty;
        }

        public string GetAuthorizedIDsUrl(Guid registratorId)
        {
            return string.Format("{0}{1}?id={2}", GetDefaultUrl(), Settings.Default.GetAuthorizedIDs, registratorId);
        }
    }
}
