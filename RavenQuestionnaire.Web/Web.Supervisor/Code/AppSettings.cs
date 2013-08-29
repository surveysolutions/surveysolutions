using WB.UI.Shared.Web;
using System.Collections.Specialized;

namespace Web.Supervisor.Code
{
    public sealed class AppSettings : WebConfigHelper
    {
        public static readonly AppSettings Instance = new AppSettings(System.Configuration.ConfigurationManager.AppSettings);

        const string ADMINEMAIL = "AdminEmail";

        public string AdminEmail { get; set; }

        private AppSettings(NameValueCollection settingsCollection)
            : base(settingsCollection)
        {
            this.AdminEmail = this.GetString(ADMINEMAIL);
        }
    }
}