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

        public string MultipassKey { get; set; }
        public string ReturnUrlFormat { get; set; }
        public string SiteKey { get; set; }
    }
}
