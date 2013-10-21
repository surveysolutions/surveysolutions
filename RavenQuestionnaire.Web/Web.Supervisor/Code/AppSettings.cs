using WB.UI.Shared.Web;
using System.Collections.Specialized;

namespace Web.Supervisor.Code
{
    public sealed class AppSettings : WebConfigHelper
    {
        public static readonly AppSettings Instance = new AppSettings(System.Configuration.ConfigurationManager.AppSettings);

        public static bool IsDebugBuilded
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

        const string ADMINEMAIL = "AdminEmail";
        const string ACCEPTUNSIGNEDCERTIFICATE = "AcceptUnsignedCertificate";

        public string AdminEmail { get; set; }
        public bool AcceptUnsignedCertificate { get; set; }

        private AppSettings(NameValueCollection settingsCollection)
            : base(settingsCollection)
        {
            this.AdminEmail = this.GetString(ADMINEMAIL);
            this.AcceptUnsignedCertificate = this.GetBoolean(ACCEPTUNSIGNEDCERTIFICATE, true);
        }
    }
}