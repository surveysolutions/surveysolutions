using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf
{
    public class DeskSettings
    {
        public DeskSettings()
        {
        }

        public DeskSettings(string multipassKey, string returnUrlFormat, string siteKey)
        {
            MultipassKey = multipassKey;
            ReturnUrlFormat = returnUrlFormat;
            SiteKey = siteKey;
        }

        public string MultipassKey { get; set; } = String.Empty;
        public string ReturnUrlFormat { get; set; } = String.Empty;
        public string SiteKey { get; set; } = String.Empty;
    }
}
