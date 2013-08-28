﻿using WB.UI.Shared.Web;

namespace WB.UI.Designer
{
    
    using System.Collections.Specialized;
    using System.Configuration;

    public sealed class AppSettings : WebConfigHelper
    {
        public static readonly AppSettings Instance = new AppSettings(ConfigurationManager.AppSettings);

        const string ISRECATPCHAENABLED = "IsReCaptchaEnabled";
        const string RAVENDOCUMENTSTORE = "Raven.DocumentStore";
        const string RAVENUSERNAME = "Raven.Username";
        const string RAVENUSERPASSWORD = "Raven.Password";
        const string WKHTMLTOPDFEXECUTABLEPATH = "WKHtmlToPdfExecutablePath";
        const string ISTRACKINGENABLED = "IsTrackingEnabled";
        const string STORAGELOADINGCHUNKSIZE = "StorageLoadingChunkSize";
        const string SUPPORTEMAIL = "SupportEmail";

        public bool IsReCaptchaEnabled { get; private set; }
        public string RavenDocumentStore { get; private set; }
        public string RavenUserName { get; private set; }
        public string RavenUserPassword { get; private set; }
        public string WKHtmlToPdfExecutablePath { get; private set; }
        public bool IsTrackingEnabled { get; private set; }
        public int StorageLoadingChunkSize { get; private set; }
        public string SupportEmail { get; set; }

        private AppSettings(NameValueCollection settingsCollection)
            : base(settingsCollection)
        {
            IsReCaptchaEnabled = this.GetBoolean(ISRECATPCHAENABLED, true);
            RavenDocumentStore = this.GetString(RAVENDOCUMENTSTORE);
            RavenUserName = this.GetString(RAVENUSERNAME);
            RavenUserPassword = this.GetString(RAVENUSERPASSWORD);
            WKHtmlToPdfExecutablePath = this.GetString(WKHTMLTOPDFEXECUTABLEPATH);
            IsTrackingEnabled = this.GetBoolean(ISTRACKINGENABLED, false);
            StorageLoadingChunkSize = this.GetInt(STORAGELOADINGCHUNKSIZE, 1024);
            SupportEmail = this.GetString(SUPPORTEMAIL);
        }
    }
}