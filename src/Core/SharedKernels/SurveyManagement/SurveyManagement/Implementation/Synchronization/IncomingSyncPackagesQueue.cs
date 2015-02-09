using System;
using System.Diagnostics;
using System.Linq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization
{
    internal class IncomingSyncPackagesQueue : IIncomingSyncPackagesQueue
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string incomingUnprocessedPackagesDirectory;
        private readonly SyncSettings syncSettings;

        public IncomingSyncPackagesQueue(IFileSystemAccessor fileSystemAccessor, SyncSettings syncSettings)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.syncSettings = syncSettings;

            this.incomingUnprocessedPackagesDirectory = fileSystemAccessor.CombinePath(syncSettings.AppDataDirectory,
                syncSettings.IncomingUnprocessedPackagesDirectoryName);

            if (!fileSystemAccessor.IsDirectoryExists(this.incomingUnprocessedPackagesDirectory))
                fileSystemAccessor.CreateDirectory(this.incomingUnprocessedPackagesDirectory);
        }

        public void PushSyncItem(string item)
        {
            if (string.IsNullOrWhiteSpace(item))
                throw new ArgumentException("Sync Item is not set.");

            string syncPackageFileName = string.Format("{0}.{1}", Guid.NewGuid().FormatGuid(), this.syncSettings.IncomingCapiPackageFileNameExtension);
            string fullPathToSyncPackage = this.fileSystemAccessor.CombinePath(this.incomingUnprocessedPackagesDirectory, syncPackageFileName);

            this.fileSystemAccessor.WriteAllText(fullPathToSyncPackage, item);
        }

        public int QueueLength
        {
            get
            {
                return this.fileSystemAccessor.GetFilesInDirectory(this.incomingUnprocessedPackagesDirectory).Count();
            }
        }

        public string DeQueue()
        {
            return this.fileSystemAccessor.GetFilesInDirectory(this.incomingUnprocessedPackagesDirectory).FirstOrDefault();
        }
    }
}
