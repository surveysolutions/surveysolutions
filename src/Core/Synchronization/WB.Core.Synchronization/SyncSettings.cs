namespace WB.Core.Synchronization
{
    public class SyncSettings
    {
        public SyncSettings(bool reevaluateInterviewWhenSynchronized, string appDataDirectory, string incomingCapiPackagesWithErrorsDirectoryName,
            string incomingCapiPackageFileNameExtension, string incomingUnprocessedPackagesDirectoryName)
        {
            this.ReevaluateInterviewWhenSynchronized = reevaluateInterviewWhenSynchronized;
            this.AppDataDirectory = appDataDirectory;
            this.IncomingCapiPackagesWithErrorsDirectoryName = incomingCapiPackagesWithErrorsDirectoryName;
            this.IncomingCapiPackageFileNameExtension = incomingCapiPackageFileNameExtension;
            IncomingUnprocessedPackagesDirectoryName = incomingUnprocessedPackagesDirectoryName;
        }

        public bool ReevaluateInterviewWhenSynchronized { get; private set; }
        public string AppDataDirectory { get; private set; }
        public string IncomingUnprocessedPackagesDirectoryName { get; private set; }
        public string IncomingCapiPackagesWithErrorsDirectoryName { get; private set; }
        public string IncomingCapiPackageFileNameExtension { get; private set; }
    }
}