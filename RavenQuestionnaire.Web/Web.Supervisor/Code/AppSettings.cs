using System.Configuration;
using WB.UI.Shared.Web;
using System.Collections.Specialized;
using WB.UI.Shared.Web.Extensions;

namespace Web.Supervisor.Code
{
    public sealed class AppSettings
    {
        public static readonly AppSettings Instance = new AppSettings();

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

        public string AdminEmail
        {
            get { return ConfigurationManager.AppSettings.GetString("AdminEmail"); }
        }

        public bool AcceptUnsignedCertificate
        {
            get { return ConfigurationManager.AppSettings.GetBool("AcceptUnsignedCertificate", true); }
        }
    }
}