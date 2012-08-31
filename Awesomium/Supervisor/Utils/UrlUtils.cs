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
            return Settings.Default.DefaultUrl + Settings.Default.LoginPath;
        }

        public string GetAuthentificationCheckUrl()
        {
            return string.Format("{0}{1}", Settings.Default.DefaultUrl, Settings.Default.AuthentificationCheckPath);
        }

        public string GetEnpointUrl()
        {
            return Settings.Default.EndpointExportPath;
        }

        public string GetUsbPushUrl(Guid clientId)
        {
            return string.Format("{0}{1}?syncKey={2}", Settings.Default.DefaultUrl, Settings.Default.UsbImportPath,
                                 clientId);
        }


        public string GetCheckPushPrerequisitesUrl()
        {
            return string.Format("{0}{1}", Settings.Default.DefaultUrl,
                                 Settings.Default.CheckEventPath);
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
            throw new NotImplementedException();
        }
    }
}
