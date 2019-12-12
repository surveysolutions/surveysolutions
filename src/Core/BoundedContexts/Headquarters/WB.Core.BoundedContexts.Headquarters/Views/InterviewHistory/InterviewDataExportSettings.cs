namespace WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory
{
    public class InterviewDataExportSettings
    {
        public InterviewDataExportSettings()
        {
        }

        public InterviewDataExportSettings(
            string exportServiceUrl,
            int limitOfCachedItemsByDenormalizer)
        {
            this.ExportServiceUrl = exportServiceUrl;
            this.LimitOfCachedItemsByDenormalizer = limitOfCachedItemsByDenormalizer;
        }

        public string ExportServiceUrl { get; private set; }

        public int LimitOfCachedItemsByDenormalizer { get; private set; } = 100;
    }
}
