namespace WB.Core.BoundedContexts.Headquarters.DataExport
{
    public class ExportSettings
    {
        public ExportSettings(int backgroundExportIntervalInSeconds)
        {
            this.BackgroundExportIntervalInSeconds = backgroundExportIntervalInSeconds;
        }

        public int BackgroundExportIntervalInSeconds { get; }
    }
}