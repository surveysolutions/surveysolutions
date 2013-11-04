using WB.UI.Shared.Web;
using System.Collections.Specialized;

namespace Web.Supervisor.Code
{
    public sealed class AppSettings : WebConfigHelper
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
            get { return this.GetString("AdminEmail"); }
        }

        public bool AcceptUnsignedCertificate
        {
            get { return this.GetBoolean("AcceptUnsignedCertificate", true); }
        }
    }
}