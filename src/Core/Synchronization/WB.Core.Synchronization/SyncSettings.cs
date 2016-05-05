using System;

namespace WB.Core.Synchronization
{
    public class SyncSettings
    {
        public SyncSettings()
        {
        }

        public SyncSettings(string origin)
        {
            this.Origin = origin;
        }

        public SyncSettings(string appDataDirectory, string incomingCapiPackagesWithErrorsDirectoryName,
            string incomingCapiPackageFileNameExtension, string incomingUnprocessedPackagesDirectoryName, string origin,
            int retryCount, int retryIntervalInSeconds, bool useBackgroundJobForProcessingPackages)
        {
            this.AppDataDirectory = appDataDirectory;
            this.IncomingCapiPackagesWithErrorsDirectoryName = incomingCapiPackagesWithErrorsDirectoryName;
            this.IncomingCapiPackageFileNameExtension = incomingCapiPackageFileNameExtension;
            this.IncomingUnprocessedPackagesDirectoryName = incomingUnprocessedPackagesDirectoryName;
            this.Origin = origin;
            this.RetryCount = retryCount;
            this.RetryIntervalInSeconds = retryIntervalInSeconds;
            this.UseBackgroundJobForProcessingPackages = useBackgroundJobForProcessingPackages;
        }

        [Obsolete("Since v 5.8")]
        public string AppDataDirectory { get; private set; }
        [Obsolete("Since v 5.8")]
        public string IncomingUnprocessedPackagesDirectoryName { get; private set; }
        [Obsolete("Since v 5.8")]
        public string IncomingCapiPackagesWithErrorsDirectoryName { get; private set; }
        [Obsolete("Since v 5.8")]
        public string IncomingCapiPackageFileNameExtension { get; private set; }
        [Obsolete("Since v 5.8")]
        public int RetryCount { get; private set; }
        [Obsolete("Since v 5.8")]
        public int RetryIntervalInSeconds { get; private set; }

        public string Origin { get; private set; }

        public bool UseBackgroundJobForProcessingPackages { get; set; }
    }
}