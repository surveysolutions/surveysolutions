using System;
using System.Linq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization.IncomePackagesRepository
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

            incomingUnprocessedPackagesDirectory = fileSystemAccessor.CombinePath(syncSettings.AppDataDirectory,
                syncSettings.IncomingUnprocessedPackagesDirectoryName);

            if (!fileSystemAccessor.IsDirectoryExists(incomingUnprocessedPackagesDirectory))
                fileSystemAccessor.CreateDirectory(incomingUnprocessedPackagesDirectory);
        }

        public void PushSyncItem(string item)
        {
            if (string.IsNullOrWhiteSpace(item))
                throw new ArgumentException("Sync Item is not set.");

            this.fileSystemAccessor.WriteAllText(
                this.fileSystemAccessor.CombinePath(this.incomingUnprocessedPackagesDirectory,
                    string.Format("{0}.{1}", Guid.NewGuid().FormatGuid(),
                        this.syncSettings.IncomingCapiPackageFileNameExtension)), item);

        }

        public int QueueLength
        {
            get { return fileSystemAccessor.GetFilesInDirectory(incomingUnprocessedPackagesDirectory).Count(); }
        }

        public string DeQueue()
        {
            return fileSystemAccessor.GetFilesInDirectory(incomingUnprocessedPackagesDirectory).FirstOrDefault();
        }
    }
}
