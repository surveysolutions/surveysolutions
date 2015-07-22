using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Main.Core.Events;
using Polly;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization
{
    internal class IncomingSyncPackagesQueue : IIncomingSyncPackagesQueue
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string incomingUnprocessedPackagesDirectory;
        private readonly SyncSettings syncSettings;
        private readonly ILogger logger;
        private readonly IJsonUtils jsonUtils;
        private readonly IArchiveUtils archiver;

        public IncomingSyncPackagesQueue(IFileSystemAccessor fileSystemAccessor, 
            SyncSettings syncSettings, 
            ILogger logger, 
            IJsonUtils jsonUtils, 
            IArchiveUtils archiver)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.syncSettings = syncSettings;
            this.logger = logger;
            this.jsonUtils = jsonUtils;
            this.archiver = archiver;
            this.incomingUnprocessedPackagesDirectory = fileSystemAccessor.CombinePath(syncSettings.AppDataDirectory,
                syncSettings.IncomingUnprocessedPackagesDirectoryName);

            if (!fileSystemAccessor.IsDirectoryExists(this.incomingUnprocessedPackagesDirectory))
                fileSystemAccessor.CreateDirectory(this.incomingUnprocessedPackagesDirectory);
        }

        public void Enqueue(Guid interviewId, string item)
        {
            if (string.IsNullOrWhiteSpace(item))
                throw new ArgumentException("Sync Item is not set.");

            string syncPackageFileName = string.Format("{0}-{1}.{2}", DateTime.Now.Ticks, interviewId.FormatGuid(), this.syncSettings.IncomingCapiPackageFileNameExtension);
            string fullPathToSyncPackage = this.fileSystemAccessor.CombinePath(this.incomingUnprocessedPackagesDirectory, syncPackageFileName);

            this.fileSystemAccessor.WriteAllText(fullPathToSyncPackage, item);
        }

        public int QueueLength
        {
            get
            {
                return
                    this.fileSystemAccessor.GetFilesInDirectory(this.incomingUnprocessedPackagesDirectory,
                        string.Format("*{0}", this.syncSettings.IncomingCapiPackageFileNameExtension)).Count();
            }
        }

        public IncomingSyncPackage DeQueue()
        {
            var pathToPackage = fileSystemAccessor.GetFilesInDirectory(incomingUnprocessedPackagesDirectory).FirstOrDefault();

            if (string.IsNullOrEmpty(pathToPackage) || !fileSystemAccessor.IsFileExists(pathToPackage))
                return null;

            Guid? interviewId = null;
            try
            {
                var policy = SetupRetryPolicyForPackage(pathToPackage);
        
                var fileContent = policy.Execute(() => fileSystemAccessor.ReadAllText(pathToPackage));

                var syncItem = jsonUtils.Deserialize<SyncItem>(fileContent);

                interviewId = syncItem.RootId;

                var meta =
                    jsonUtils.Deserialize<InterviewMetaInfo>(archiver.DecompressString(syncItem.MetaInfo));

                var eventsToSync =
                    jsonUtils.Deserialize<AggregateRootEvent[]>(archiver.DecompressString(syncItem.Content))
                        .Select(e => e.Payload)
                        .ToArray();

                return new IncomingSyncPackage(meta.PublicKey, meta.ResponsibleId, meta.TemplateId,
                        meta.TemplateVersion, (InterviewStatus)meta.Status, eventsToSync, meta.CreatedOnClient ?? false, syncSettings.Origin, pathToPackage);
            }
            catch (Exception e)
            {
                var message = string.Format("package '{0}' wasn't parsed. Reason: '{1}'", pathToPackage, e.Message);
                logger.Error(message, e);
                throw new IncomingSyncPackageException(message, e, interviewId, pathToPackage);
            }
        }

        private ContextualPolicy SetupRetryPolicyForPackage(string pathToPackage)
        {
            return Policy
                .Handle<Win32Exception>()
                .WaitAndRetry(
                    syncSettings.RetryCount,
                    retryAttempt => TimeSpan.FromSeconds(syncSettings.RetryIntervalInSeconds),
                    (exception, retryCount, context) =>
                    {
                        logger.Warn(
                            string.Format("package '{0}' failed to open with error '{1}'", pathToPackage, exception.Message),
                            exception);
                    }
                );
        }

        public void DeleteSyncItem(string syncItemPath)
        {
            fileSystemAccessor.DeleteFile(syncItemPath);
        }

        public bool HasPackagesByInterviewId(Guid interviewId)
        {
            return
                this.fileSystemAccessor.GetFilesInDirectory(this.incomingUnprocessedPackagesDirectory,
                    string.Format("*{0}*{1}", interviewId.FormatGuid(),
                        this.syncSettings.IncomingCapiPackageFileNameExtension)).Any();
        }
    }
}