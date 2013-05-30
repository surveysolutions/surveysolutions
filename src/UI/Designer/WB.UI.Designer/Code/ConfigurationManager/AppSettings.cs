namespace WB.UI.Designer
{
    using System.Collections.Specialized;
    using System.Configuration;

    public sealed class AppSettings: WebConfigHelper
    {
        public static readonly AppSettings Instance = new AppSettings(ConfigurationManager.AppSettings);

        const string ISRECATPCHAENABLED = "IsReCaptchaEnabled";
        const string RAVENDOCUMENTSTORE = "Raven.DocumentStore";
        const string WKHTMLTOPDFEXECUTABLEPATH = "WKHtmlToPdfExecutablePath";
        const string ISLOCKINGACCOUNTPOLICYFORCED = "IsLockingAccountPolicyForced";


        const string REFORMATEMAIL = "ReformatEmail";

        public bool IsReCaptchaEnabled { get; private set; }
        public string RavenDocumentStore { get; private set; }
        public string WKHtmlToPdfExecutablePath { get; private set; }
        public bool IsLockingAccountPolicyForced { get; private set; }

        public bool ReformatEmail { get; private set; }

        private AppSettings(NameValueCollection settingsCollection)
            : base(settingsCollection)
        {
            IsReCaptchaEnabled = GetBoolean(ISRECATPCHAENABLED, true);
            RavenDocumentStore = this.GetString(RAVENDOCUMENTSTORE);
            WKHtmlToPdfExecutablePath = this.GetString(WKHTMLTOPDFEXECUTABLEPATH);
            IsLockingAccountPolicyForced = GetBoolean(ISLOCKINGACCOUNTPOLICYFORCED, true);

            ReformatEmail = this.GetBoolean(REFORMATEMAIL, true);
        }
    }
}