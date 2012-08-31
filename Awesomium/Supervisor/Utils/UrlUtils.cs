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

        public string GetPushUrl(Guid clientId)
        {
            return string.Format("{0}{1}?url={2}&syncKey={3}", Settings.Default.DefaultUrl,
                                 Settings.Default.NetworkLocalExportPath, Settings.Default.EndpointExportPath,
                                 clientId);
        }

        public string GetPullUrl(Guid clientId)
        {
            return string.Format("{0}{1}?url={2}&syncKey={3}", Settings.Default.DefaultUrl,
                                 Settings.Default.NetworkLocalImportPath, Settings.Default.EndpointExportPath, clientId);
        }

        public string GetPushCheckStateUrl(Guid processid)
        {
            return string.Format("{0}{1}?id={2}", Settings.Default.DefaultUrl, Settings.Default.NetworkCheckStatePath,
                                 processid);
        }

        public string GetEnpointUrl()
        {
            return Settings.Default.EndpointExportPath;
        }

        public string GetUsbPushUrl(Guid clientId)
        {
            return string.Format("{0}{1}?syncKey={2}", Settings.Default.DefaultUrl, Settings.Default.UsbExportPath,
                                 clientId);
        }

        public string GetUsbPullUrl(Guid clientId)
        {
            return string.Format("{0}{1}", Settings.Default.DefaultUrl, Settings.Default.UsbImportPath);
        }

        public string GetCheckPushPrerequisitesUrl()
        {
            return string.Format("{0}{1}", Settings.Default.DefaultUrl,
                                 Settings.Default.CheckEventPath);
        }

        public string GetCheckNoCompletedTemplatesForCapiUrl()
        {
            return string.Format("{0}{1}", Settings.Default.DefaultUrl,
                                 Settings.Default.NetworkSelectNoCompletedQuestionnaire);
        }
    }
}
