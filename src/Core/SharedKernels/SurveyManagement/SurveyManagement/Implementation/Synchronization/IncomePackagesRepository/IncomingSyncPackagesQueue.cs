using System;
using System.Linq;
using Main.Core.Events;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization.IncomePackagesRepository
{
    internal class IncomingSyncPackagesQueue : IIncomingSyncPackagesQueue
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string incomingUnprocessedPackagesDirectory;
        private readonly SyncSettings syncSettings;
        private readonly ILogger logger;
        private readonly IJsonUtils jsonUtils;
        private readonly IArchiveUtils archiver;
        private IUnhandledPackageStorage unhandledPackageStorage;

        public IncomingSyncPackagesQueue(IFileSystemAccessor fileSystemAccessor, SyncSettings syncSettings, ILogger logger, IJsonUtils jsonUtils, IArchiveUtils archiver, IUnhandledPackageStorage unhandledPackageStorage)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.syncSettings = syncSettings;
            this.logger = logger;
            this.jsonUtils = jsonUtils;
            this.archiver = archiver;
            this.unhandledPackageStorage = unhandledPackageStorage;

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

        public IncomingSyncPackages DeQueue()
        {
            var pathToPackage = fileSystemAccessor.GetFilesInDirectory(incomingUnprocessedPackagesDirectory).FirstOrDefault();

            if (string.IsNullOrEmpty(pathToPackage) || !fileSystemAccessor.IsFileExists(pathToPackage))
                return null;

            Guid? interviewId = null;
            try
            {
                var syncItem = jsonUtils.Deserialize<SyncItem>(fileSystemAccessor.ReadAllText(pathToPackage));

                interviewId = syncItem.RootId;

                var meta =
                    jsonUtils.Deserialize<InterviewMetaInfo>(archiver.DecompressString(syncItem.MetaInfo));

                var eventsToSync =
                    jsonUtils.Deserialize<AggregateRootEvent[]>(archiver.DecompressString(syncItem.Content))
                        .Select(e => e.Payload)
                        .ToArray();

                return new IncomingSyncPackages(meta.PublicKey, meta.ResponsibleId, meta.TemplateId,
                        meta.TemplateVersion, (InterviewStatus)meta.Status, eventsToSync, meta.CreatedOnClient ?? false, syncSettings.Origin, pathToPackage);
            }
            catch (Exception e)
            {
                logger.Error(string.Format("package '{0}' wasn't parsed. Reason: '{1}'", pathToPackage, e.Message), e);
                unhandledPackageStorage.StoreUnhandledPackage(pathToPackage, interviewId);
                DeleteSyncItem(pathToPackage);
            }
            return null;
        }

        public void DeleteSyncItem(string syncItemPath)
        {
            fileSystemAccessor.DeleteFile(syncItemPath);
        }
    }
}
