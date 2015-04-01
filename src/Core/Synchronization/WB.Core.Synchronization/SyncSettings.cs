namespace WB.Core.Synchronization
{
    public class SyncSettings
    {
        public SyncSettings(string appDataDirectory, string incomingCapiPackagesWithErrorsDirectoryName,
            string incomingCapiPackageFileNameExtension, string incomingUnprocessedPackagesDirectoryName, string origin, int retryCount, int retryInterval)
        {
            this.AppDataDirectory = appDataDirectory;
            this.IncomingCapiPackagesWithErrorsDirectoryName = incomingCapiPackagesWithErrorsDirectoryName;
            this.IncomingCapiPackageFileNameExtension = incomingCapiPackageFileNameExtension;
            this.IncomingUnprocessedPackagesDirectoryName = incomingUnprocessedPackagesDirectoryName;
            this.Origin = origin;
            this.RetryCount = retryCount;
            this.RetryInterval = retryInterval;
        }

        public string AppDataDirectory { get; private set; }
        public string IncomingUnprocessedPackagesDirectoryName { get; private set; }
        public string IncomingCapiPackagesWithErrorsDirectoryName { get; private set; }
        public string IncomingCapiPackageFileNameExtension { get; private set; }
        public string Origin { get; private set; }
        public int RetryCount { get; private set; }
        public int RetryInterval { get; private set; }
    }
}