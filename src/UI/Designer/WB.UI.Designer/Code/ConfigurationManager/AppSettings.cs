using System.Configuration;
using WB.UI.Shared.Web;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Designer
{
    public sealed class AppSettings
    {
        public static bool IsDebugRelease
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        public static readonly AppSettings Instance = new AppSettings();

        public bool IsReCaptchaEnabled
        {
            get { return ConfigurationManager.AppSettings.GetBool("IsReCaptchaEnabled", true); }
        }

        public string RavenDocumentStore
        {
            get { return ConfigurationManager.AppSettings.GetString("Raven.DocumentStore"); }
        }

        public string RavenUserName
        {
            get { return ConfigurationManager.AppSettings.GetString("Raven.Username"); }
        }

        public string RavenUserPassword
        {
            get { return ConfigurationManager.AppSettings.GetString("Raven.Password"); }
        }

        public string WKHtmlToPdfExecutablePath
        {
            get { return ConfigurationManager.AppSettings.GetString("WKHtmlToPdfExecutablePath"); }
        }

        public bool IsTrackingEnabled
        {
            get { return ConfigurationManager.AppSettings.GetBool("IsTrackingEnabled", false); }
        }

        public int StorageLoadingChunkSize
        {
            get { return ConfigurationManager.AppSettings.GetInt("StorageLoadingChunkSize", 1024); }
        }

        public string SupportEmail
        {
            get { return ConfigurationManager.AppSettings.GetString("SupportEmail"); }
        }
    }
}