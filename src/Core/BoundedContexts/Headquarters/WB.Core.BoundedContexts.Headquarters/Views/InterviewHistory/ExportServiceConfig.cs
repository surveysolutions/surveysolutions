namespace WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory
{
    public class ExportServiceConfig
    {
        public ExportMode ExportMode { get; set; }
        public int ExportPort { get; set; } = 0;
        public string ExportServiceUrl { get; set; }
    }

    public enum ExportMode
    {
        Auto = 0, // default
        Client,   // don't lunch export, use url to use export 
        Provider, // open export outside by port
    }
}
