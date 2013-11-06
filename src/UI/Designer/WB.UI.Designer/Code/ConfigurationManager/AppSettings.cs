using WB.UI.Shared.Web;

namespace WB.UI.Designer
{
    public sealed class AppSettings : WebConfigHelper
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
            get { return this.GetBoolean("IsReCaptchaEnabled", true); }
        }

        public string RavenDocumentStore
        {
            get { return this.GetString("Raven.DocumentStore"); }
        }

        public string RavenUserName
        {
            get { return this.GetString("Raven.Username"); }
        }

        public string RavenUserPassword
        {
            get { return this.GetString("Raven.Password"); }
        }

        public string WKHtmlToPdfExecutablePath
        {
            get { return this.GetString("WKHtmlToPdfExecutablePath"); }
        }

        public bool IsTrackingEnabled
        {
            get { return this.GetBoolean("IsTrackingEnabled", false); }
        }

        public int StorageLoadingChunkSize
        {
            get { return this.GetInt("StorageLoadingChunkSize", 1024); }
        }

        public string SupportEmail
        {
            get { return this.GetString("SupportEmail"); }
        }
    }
}