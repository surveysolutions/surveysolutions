namespace WB.UI.Designer
{
    public sealed class AppSettings: WebConfigHelper
    {
        public static readonly AppSettings Instance = new AppSettings();

        const string ISRECATPCHAENABLED = "IsReCaptchaEnabled";
        const string RAVENDOCUMENTSTORE = "Raven.DocumentStore";
        const string WKHTMLTOPDFEXECUTABLEPATH = "WKHtmlToPdfExecutablePath";
        const string ISLOCKINGACCOUNTPOLICYFORCED = "IsLockingAccountPolicyForced";
        const string ISANTIFORGERYENABLED = "IsAntiForgeryEnabled";


        public bool IsReCaptchaEnabled { get; private set; }
        public string RavenDocumentStore { get; private set; }
        public string WKHtmlToPdfExecutablePath { get; private set; }
        public bool IsLockingAccountPolicyForced { get; private set; }
        public bool IsAntiForgeryEnabled { get; private set; }
        

        private AppSettings()
        {
            IsReCaptchaEnabled = GetBoolean(ISRECATPCHAENABLED, true);
            RavenDocumentStore = GetString(RAVENDOCUMENTSTORE);
            WKHtmlToPdfExecutablePath = GetString(WKHTMLTOPDFEXECUTABLEPATH);
            IsLockingAccountPolicyForced = GetBoolean(ISLOCKINGACCOUNTPOLICYFORCED, true);
            IsAntiForgeryEnabled = this.GetBoolean(ISANTIFORGERYENABLED, true);
        }
    }
}