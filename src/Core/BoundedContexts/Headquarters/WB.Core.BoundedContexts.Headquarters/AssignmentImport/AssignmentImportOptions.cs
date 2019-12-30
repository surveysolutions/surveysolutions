namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public class AssignmentImportOptions
    {
        public AssignmentImportOptions(int backgroundExportIntervalInSeconds)
        {
            this.BackgroundExportIntervalInSeconds = backgroundExportIntervalInSeconds;
        }

        public int BackgroundExportIntervalInSeconds { get; } = 15;
    }
}
