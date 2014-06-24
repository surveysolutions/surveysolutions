namespace WB.Core.Synchronization
{
    public class SyncSettings
    {
        public SyncSettings(bool reevaluateInterviewWhenSynchronized, string appDataDirectory,
            string incomingCapiPackagesDirectoryName, string incomingCapiPackagesWithErrorsDirectoryName,
            string incomingCapiPackageFileNameExtension)
        {
            this.ReevaluateInterviewWhenSynchronized = reevaluateInterviewWhenSynchronized;
            this.AppDataDirectory = appDataDirectory;
            this.IncomingCapiPackagesDirectoryName = incomingCapiPackagesDirectoryName;
            this.IncomingCapiPackagesWithErrorsDirectoryName = incomingCapiPackagesWithErrorsDirectoryName;
            this.IncomingCapiPackageFileNameExtension = incomingCapiPackageFileNameExtension;
        }

        public bool ReevaluateInterviewWhenSynchronized { get; private set; }
        public string AppDataDirectory { get; private set; }
        public string IncomingCapiPackagesDirectoryName { get; private set; }
        public string IncomingCapiPackagesWithErrorsDirectoryName { get; private set; }
        public string IncomingCapiPackageFileNameExtension { get; private set; }
    }
}