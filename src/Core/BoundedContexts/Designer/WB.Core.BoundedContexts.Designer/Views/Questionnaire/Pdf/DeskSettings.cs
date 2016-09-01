namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf
{
    public class DeskSettings
    {
        public string MultipassKey { get; set; }
        public string ReturnUrlFormat { get; set; }
        public string SiteKey { get; set; }

        public DeskSettings(string multipassKey, string returnUrlFormat, string siteKey)
        {
            this.MultipassKey = multipassKey;
            this.ReturnUrlFormat = returnUrlFormat;
            this.SiteKey = siteKey;
        }
    }
}