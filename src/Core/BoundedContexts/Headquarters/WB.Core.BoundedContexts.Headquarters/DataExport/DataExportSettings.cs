namespace WB.Core.BoundedContexts.Headquarters.DataExport
{
    public class DataExportSettings
    {
        public DataExportSettings(int dataExportIntervalInSeconds)
        {
            this.DataExportIntervalInSeconds = dataExportIntervalInSeconds;
        }

        public int DataExportIntervalInSeconds { get; private set; }
    }
}